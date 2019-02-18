﻿using System;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FireSharp;
using FireSharp.Config;
using FireSharp.Interfaces;
using LiteDB;
using Newtonsoft.Json;
using Translit.Models.Other;
using Translit.Properties;

namespace Translit.Models.Windows
{
    internal class MainModel : IMainModel
    {
        public MainModel()
        {
            ConnectionString = ConfigurationManager.ConnectionStrings["LiteDatabaseConnection"].ConnectionString;

            JsonConvert.DeserializeObject<User>(Rc4.Calc(Settings.Default.FingerPrint, Settings.Default.User));
            CheckAndDownloadUpdate();
            DownlaodUpdater();
            CheckPermission();
        }

        public static string ConnectionString { get; set; }

        public int SignIn(string login, string password)
        {
            var link = $"http://account.osmium.kz/api/auth?login={login}&pass={password}";

            var request = (HttpWebRequest) WebRequest.Create(link);

            HttpWebResponse response;

            try
            {
                // Запрашиваем ответ от сервера
                response = (HttpWebResponse) request.GetResponse();
            }
            catch (Exception)
            {
                // ignored
                return 0;
            }

            var responseStream = response.GetResponseStream();

            if (responseStream == null) return 0;

            // Считываем данные
            using (var stream = new StreamReader(responseStream, Encoding.UTF8))
            {
                // Получаем объект из JSON
                var userJson = Settings.Default.User = stream.ReadToEnd();

                if (userJson == "") return 1;

                // Сохраняем уникальный ключ
                Settings.Default.FingerPrint = FingerPrint.Value();
                // Сохраняем зашифрованного пользователя в настройках приложения
                Settings.Default.User = Rc4.Calc(Settings.Default.FingerPrint, userJson);
                // Даем разрешение на редактирование
                Settings.Default.PermissionToChange = true;

                Analytics.SendUserData();

                JsonConvert.DeserializeObject<User>(Rc4.Calc(Settings.Default.FingerPrint, Settings.Default.User));
                return 2;
            }
        }

        public void LogOut()
        {
            var link = $"http://account.osmium.kz/api/auth?token={User.Token}&id={User.Id}";
            var client = new HttpClient();
            client.DeleteAsync(link);

            // Удаляем профиль в настройках приложения
            Settings.Default.User = "";
            // Удаляем MAC адресс в настройках приложения
            Settings.Default.FingerPrint = "";
            // Удаляем метку авторизации
            Settings.Default.PermissionToChange = false;

            Analytics.SendUserData();

            User.Clear();
        }

        // Сравнение сохраненного MAC адреса с MAC адресом текущей машины
        public void CompareIds()
        {
            // Если уникальный ключ текущей машины совпадает с уникальным ключом последней машины
            if (FingerPrint.Value() == Settings.Default.FingerPrint) return;
            LogOut();
        }

        // Проверка и загрузка обновления
        private void CheckAndDownloadUpdate()
        {
            Task.Factory.StartNew(() =>
            {
                IFirebaseClient client = new FirebaseClient(new FirebaseConfig
                {
                    BasePath = "https://translit-10dad.firebaseio.com/",
                    AuthSecret = "1V3A4v70wJZeZuvh4VwDmeV562zDSjuF4qDnrqtF"
                });

                ProgramUrl pu;

                try
                {
                    pu = client.Get("Update").ResultAs<ProgramUrl>();
                }
                catch (Exception)
                {
                    // ignored
                    return;
                }

                if (pu == null) return;

                var version = Assembly.GetExecutingAssembly().GetName().Version;

                var currentVersion = int.Parse($"{version.Major}{version.Minor}");
                var newVersion = int.Parse(pu.Version.Replace(".", ""));

                if (currentVersion >= newVersion) return;

                try
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    var webClient = new WebClient();
                    webClient.DownloadFileCompleted += DownloadTranslitCompleted;
                    webClient.DownloadFileAsync(new Uri(pu.Url), @"Translit.tmp");
                }
                catch (Exception)
                {
                    // ignored
                }
            });
        }

        private void DownloadTranslitCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error != null) return;

            if (File.Exists(@"Translit.zip"))
                File.Replace(@"Translit.tmp", @"Translit.zip", null);
            else
                File.Move(@"Translit.tmp", @"Translit.zip");
        }

        private void DownlaodUpdater()
        {
            Task.Factory.StartNew(() =>
            {
                if (File.Exists("Updater.exe")) return;

                IFirebaseClient client = new FirebaseClient(new FirebaseConfig
                {
                    BasePath = "https://translit-10dad.firebaseio.com/",
                    AuthSecret = "1V3A4v70wJZeZuvh4VwDmeV562zDSjuF4qDnrqtF"
                });

                ProgramUrl pu;

                try
                {
                    pu = client.Get("Updater").ResultAs<ProgramUrl>();
                }
                catch (Exception)
                {
                    // ignored
                    return;
                }

                try
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    var webClient = new WebClient();
                    webClient.DownloadFileCompleted += DownloadUpdaterCompleted;
                    webClient.DownloadFileAsync(new Uri(pu.Url), @"Updater.tmp");
                }
                catch (Exception)
                {
                    // ignored
                }
            });
        }

        private void DownloadUpdaterCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error != null) return;

            File.Move(@"Updater.tmp", @"Updater.exe");
        }

        private void CheckPermission()
        {
            Task.Factory.StartNew(() =>
            {
                using (var db = new LiteDatabase(ConnectionString))
                {
                    var analytics = db.GetCollection<UserData>("Analytics");
                    var localUserData = analytics.FindAll().FirstOrDefault(u => u.Id == FingerPrint.Value());

                    if (localUserData == null) return;

                    if (localUserData.PermissionToUse) return;

                    Environment.Exit(0);
                }
            });
        }
    }
}