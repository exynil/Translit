using FireSharp;
using FireSharp.Config;
using FireSharp.Interfaces;
using LiteDB;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using Translit.Properties;

namespace Translit.Entity
{
    public class Analytics
    {
        public IFirebaseClient Client { get; set; }
        public bool Connection { get; set; }
        public UserData UserData { get; set; }
        public UserData UserDataInTheCloud { get; set; }
        public string ConnectionString { get; set; }

        public Analytics()
        {
            ConnectionString = ConfigurationManager.ConnectionStrings["LiteDatabaseConnection"].ConnectionString;
            Client = new FirebaseClient(new FirebaseConfig
            {
                BasePath = "https://translit-10dad.firebaseio.com/",
                AuthSecret = "1V3A4v70wJZeZuvh4VwDmeV562zDSjuF4qDnrqtF"
            });
            Connection = true;
        }

        public void Start()
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                var analytics = db.GetCollection<UserData>("Analytics");

                var id = FingerPrint.Value();

                UserData = analytics.FindAll().FirstOrDefault(u => u.Id == id);

                try
                {
                    UserDataInTheCloud = Client.Get($"Analytics/{id}").ResultAs<UserData>();
                }
                catch (Exception)
                {
                    Connection = false;
                    if (UserData == null)
                    {
                        UserData = new UserData();
                    }
                    return;
                }

                // Если в базе и в облаке нет аналитики
                if (UserData == null && UserDataInTheCloud == null)
                {
                    // Создаем новую аналитку для текущего пользователя
                    UserData = new UserData();
                    // Отправляем аналитику в облако
                    Client.SetAsync($"Analytics/{id}", UserData);
                    // Отправляем аналитику в локальную базу
                    analytics.Insert(UserData);
                }
                // Если в базе нет аналитики, а в облаке есть
                else if (UserData == null && UserDataInTheCloud != null)
                {
                    // Копируем аналитику из облака
                    UserData = UserDataInTheCloud;
                    // Добавляем аналитику в локальную базу
                    analytics.Insert(UserData);
                }
                // Если и в базе и в облаке есть аналитика
                else if (UserData != null && UserDataInTheCloud != null)
                {
                    // Сравниваем дату последнего изменения
                    if (UserData.LastUsedDate > UserDataInTheCloud.LastUsedDate)
                    {
                        Client.SetAsync($"Analytics/{id}", UserData);
                    }
                }
            }
        }

        public void SaveAndSendUserData()
        {
            UserData.UpdateAllData();

            try
            {
                Client.SetAsync($"Analytics/{UserData.Id}", UserData);
            }
            catch (Exception)
            {
                // ignored
            }

            using (var db = new LiteDatabase(ConnectionString))
            {
                var userData = db.GetCollection<UserData>("UserData");

                userData.Update(UserData);
            }
        }
    }
}
