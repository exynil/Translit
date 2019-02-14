using FireSharp;
using FireSharp.Config;
using FireSharp.Interfaces;
using LiteDB;
using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace Translit.Models.Other
{
    public static class Analytics
    {
        public static bool Online { get; set; }
        public static UserData UserDataLocal { get; set; }
        public static UserData UserDataCloud { get; set; }
        public static string ConnectionString { get; set; }

        public static void Start()
        {
            Task.Factory.StartNew(() =>
            {
                if (Online) return;

                ConnectionString = ConfigurationManager.ConnectionStrings["LiteDatabaseConnection"].ConnectionString;

                using (var db = new LiteDatabase(ConnectionString))
                {
                    var analytics = db.GetCollection<UserData>("Analytics");

                    UserDataLocal = analytics.FindAll().FirstOrDefault(u => u.Id == FingerPrint.Value());

                    // Пробуем загрузить аналитику из облака
                    if (LoadUserDataCloud())
                    {
                        // Если в базе нет аналитики, а в облаке есть
                        if (UserDataLocal == null && UserDataCloud != null)
                        {
                            // Копируем аналитику из облака
                            UserDataLocal = (UserData)UserDataCloud.Clone();
                            // Сбрасываем данные локального счетчика
                            UserDataLocal.Counter.Reset();
                        }
                        // Если и в базе и в облаке есть аналитика
                        else if (UserDataLocal != null && UserDataCloud != null)
                        {
                            UserDataLocal.PermissionToUse = UserDataCloud.PermissionToUse;
                        }
                    }

                    if (UserDataLocal == null)
                    {
                        UserDataLocal = new UserData();
                    }

                    if (UserDataCloud == null)
                    {
                        UserDataCloud = new UserData();
                    }
                }
                SaveAndSendUserData();
            });
        }

        public static void SaveAndSendUserData()
        {
            Task.Factory.StartNew(() =>
            {
                IFirebaseClient client = new FirebaseClient(new FirebaseConfig
                {
                    BasePath = "https://translit-10dad.firebaseio.com/",
                    AuthSecret = "1V3A4v70wJZeZuvh4VwDmeV562zDSjuF4qDnrqtF"
                });

                // Если мы в сети
                if (Online)
                {
                    // Обновляем локальные данные пользователя
                    UserDataLocal.UpdateAllData();

                    // Обновляем облачные данные пользователя
                    UserDataCloud.UpdateAllData();

                    // Прибавляем к облачному счетчику данные локального счетчика
                    UserDataCloud.Counter.Add(UserDataLocal.Counter);

                    // Пытаемся отправить данные
                    try
                    {
                        client.Set($"Analytics/{UserDataLocal.Id}", UserDataCloud);
                    }
                    catch (Exception)
                    {
                        Online = false;
                        // Если данные не удалось отправить, сохраняем их локально
                        SaveUserDataToLocalDb();
                        return;
                    }

                    // Если данные отправились сбрасываем данные локального счетчика
                    UserDataLocal.Counter.Reset();

                    using (var db = new LiteDatabase(ConnectionString))
                    {
                        var analytics = db.GetCollection<UserData>("Analytics");
                        analytics.Upsert(UserDataLocal);
                    }
                }
                // Если не в сети, пытаемся подключиться и отправить данные
                else if (LoadUserDataCloud())
                {
                    SaveAndSendUserData();
                }
                // Если оффлайн сохраняем данные локально
                else
                {
                    SaveUserDataToLocalDb();
                }
            });
        }

        // Сохраняем аналитику локально
        public static void SaveUserDataToLocalDb()
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                var analytics = db.GetCollection<UserData>("Analytics");
                analytics.Upsert(UserDataLocal);
            }
        }

        // Загружаем облачную аналитику пользователя
        public static bool LoadUserDataCloud()
        {
            IFirebaseClient client = new FirebaseClient(new FirebaseConfig
            {
                BasePath = "https://translit-10dad.firebaseio.com/",
                AuthSecret = "1V3A4v70wJZeZuvh4VwDmeV562zDSjuF4qDnrqtF"
            });

            try
            {
                UserDataCloud = client.Get($"Analytics/{FingerPrint.Value()}").ResultAs<UserData>();
                return Online = true;
            }
            catch (Exception)
            {
                return Online = false;
            }
        }
    }
}
