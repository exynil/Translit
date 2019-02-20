using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using FireSharp;
using FireSharp.Config;
using FireSharp.Interfaces;
using LiteDB;

namespace Translit.Models.Other
{
    public static class Analytics
    {
        public static bool Online { get; set; }
        public static UserData UnsentUserData { get; set; }
        public static UserData LocalUserData { get; set; }
        public static UserData CloudUserData { get; set; }
        public static string ConnectionString { get; set; }

        public static void Start()
        {
            Task.Factory.StartNew(() =>
            {
                if (Online) return;

                ConnectionString = ConfigurationManager.ConnectionStrings["LiteDbConnection"].ConnectionString;

                using (var db = new LiteDatabase(ConnectionString))
                {
                    var analytics = db.GetCollection<UserData>("Analytics");
                    LocalUserData = analytics.FindAll().FirstOrDefault(u => u.Id == FingerPrint.Value());

                    var unsentAnalytics = db.GetCollection<UserData>("UnsentAnalytics");
                    UnsentUserData = unsentAnalytics.FindAll().FirstOrDefault(u => u.Id == FingerPrint.Value());

                    // Пробуем загрузить аналитику из облака
                    LoadCloudUserData();

                    if (CloudUserData != null && Online) LocalUserData = (UserData) CloudUserData.Clone();

                    if (UnsentUserData == null) UnsentUserData = new UserData();

                    if (LocalUserData == null) LocalUserData = new UserData();

                    if (CloudUserData == null) CloudUserData = new UserData();
                }

                SendUserData();
            });
        }

        public static void SendUserData()
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
                    // Обновляем облачные данные пользователя
                    CloudUserData.UpdateAllData();

                    // Прибавляем к облачному счетчику данные неотправленного счетчика
                    CloudUserData.Counter.Add(UnsentUserData.Counter);

                    // Пытаемся отправить данные
                    try
                    {
                        client.Set($"Analytics/{LocalUserData.Id}", CloudUserData);
                    }
                    catch (Exception)
                    {
                        Online = false;
                        SaveLocalAndUnsentUserData();
                        CloudUserData.Counter.Subtract(UnsentUserData.Counter);
                        return;
                    }

                    // Если данные отправились сбрасываем данные неотправленного счетчика
                    UnsentUserData.Counter.Reset();
                    // Копируем облачные данные в локальные локальные
                    LocalUserData = (UserData) CloudUserData.Clone();
                    // Сохраняем локальную и не отправленную аналитику
                    SaveLocalAndUnsentUserData();
                }
                else if (LoadCloudUserData())
                {
                    SendUserData();
                }
                else
                {
                    SaveLocalAndUnsentUserData();
                }
            });
        }

        // Сохраняем локальную аналитику и неотправленную аналитику
        public static void SaveLocalAndUnsentUserData()
        {
            if (LocalUserData == null || UnsentUserData == null) return;

            using (var db = new LiteDatabase(ConnectionString))
            {
                var analytics = db.GetCollection<UserData>("Analytics");
                analytics.Upsert(LocalUserData);

                var unsentAnalytics = db.GetCollection<UserData>("UnsentAnalytics");
                unsentAnalytics.Upsert(UnsentUserData);
            }
        }

        // Загружаем облачную аналитику пользователя
        public static bool LoadCloudUserData()
        {
            IFirebaseClient client = new FirebaseClient(new FirebaseConfig
            {
                BasePath = "https://translit-10dad.firebaseio.com/",
                AuthSecret = "1V3A4v70wJZeZuvh4VwDmeV562zDSjuF4qDnrqtF"
            });

            try
            {
                CloudUserData = client.Get($"Analytics/{FingerPrint.Value()}").ResultAs<UserData>();
                return Online = true;
            }
            catch (Exception)
            {
                return Online = false;
            }
        }
    }
}