using FireSharp;
using FireSharp.Config;
using FireSharp.Interfaces;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IO;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Translit.Entity;
using Translit.Properties;

namespace Translit.Models.Windows
{
    class MainModel : IMainModel
	{
		public MainModel()
		{
			Task.Factory.StartNew(CheckAndDownloadUpdate);
		}

		public bool SignIn(string login, string password)
		{
			var link = $"http://account.osmium.kz/api/auth?login={login}&pass={password}";
			var request = (HttpWebRequest)WebRequest.Create(link);
			HttpWebResponse response;
			try
			{
				// Запрашиваем ответ от сервера
				response = (HttpWebResponse)request.GetResponse();
			}
			catch (Exception)
			{
				return false;
			}

			var responseStream = response.GetResponseStream();

			// Считываем данные
			using (var stream = new StreamReader(responseStream ?? throw new InvalidOperationException(), Encoding.UTF8))
			{
				// Получаем объект из JSON
				var user = JsonConvert.DeserializeObject<User>(stream.ReadToEnd());

				if (user == null) return true;

				SaveMacAddressAndUser(user);

				Settings.Default.AdminPermissions = true;
			}

			return true;
		}

		public void SaveMacAddressAndUser(User user)
		{
			var macAddress = GetMacAddress();
			// Переводим нашего пользователя в строку JSON
			var userJson = JsonConvert.SerializeObject(user);
			// Сохраняем зашифрованного пользователя в настройках приложения
			Settings.Default.User = Rc4.Calc(macAddress, userJson);
			//// Сохраняем MAC адресс
			Settings.Default.MacAddress = macAddress;
			//Settings.Default.Save();
		}

		public User GetUser()
		{
			if (Settings.Default.MacAddress == "") return null;
			var macAddress = GetMacAddress();

			// Возвращяем расшифрованного и дисериализованного пользователя
			return JsonConvert.DeserializeObject<User>(Rc4.Calc(macAddress, Settings.Default.User));
		}

		// Удаление токена
		public void DeleteToken()
		{
			var user = GetUser();
			var link = $"http://account.osmium.kz/api/auth?token={user.Token}&id={user.Id}";
			var client = new HttpClient();
			client.DeleteAsync(link);
		}

		// Удаление информации о пользователе из настроек
		public void DeleteUserFromSettings()
		{
			// Удаляем профиль в настройках приложения
			Settings.Default.User = "";
			// Удаляем MAC адресс в настройках приложения
			Settings.Default.MacAddress = "";
			// Удаляем метку авторизации
			Settings.Default.AdminPermissions = false;
		}

		// Сравнение сохраненного MAC адреса с MAC адресом текущей машины
		public void MacAddressComparison()
		{
			// Получаем MAC адрес машины клиента
			var macAddress = GetMacAddress();

			// Если MAC адрес текущей машины совпадает с MAC адресом последней машины
			if (macAddress == Settings.Default.MacAddress) return;
			DeleteUserFromSettings();
		}

	    public string GetMacAddress()
	    {
	        var mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
	        var moc = mc.GetInstances();
	        var macAddress = string.Empty;
	        foreach (var mo in moc)
	        {
	            if (macAddress == string.Empty)  // only return MAC Address from first card
	            {
	                if ((bool)mo["IPEnabled"]) macAddress = mo["MacAddress"].ToString();
	            }
	            mo.Dispose();
	        }
	        macAddress = macAddress.Replace(":", "");
	        return macAddress;
	    }

        private void CheckAndDownloadUpdate()
		{
			IFirebaseConfig config = new FirebaseConfig
			{
				BasePath = "https://translit-10dad.firebaseio.com/",
				AuthSecret = "1V3A4v70wJZeZuvh4VwDmeV562zDSjuF4qDnrqtF"
            };

			IFirebaseClient client = new FirebaseClient(config);

		    UpdateInfo updateInfo;

		    try
		    {
		        updateInfo = client.Get("Update").ResultAs<UpdateInfo>();
		    }
		    catch (Exception)
		    {
                return;
		    }

            if (updateInfo == null) return;

			var version = Assembly.GetExecutingAssembly().GetName().Version;

			var currentVersion = int.Parse($"{ version.Major}{ version.Minor}");
			var newVersion = int.Parse(updateInfo.Version.Replace(".", ""));

		    if (currentVersion >= newVersion) return;

		    Settings.Default.UpdateReady = false;

		    try
		    {
		        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
		        var webClient = new WebClient();
		        webClient.DownloadFileCompleted += Completed;
		        webClient.DownloadFileAsync(new Uri(updateInfo.Url), @"Translit.tmp");
		    }
		    catch (Exception)
		    {
		        // ignored
		    }
		}

		private void Completed(object sender, AsyncCompletedEventArgs e)
		{
		    if (e.Error != null) return;

            if (File.Exists(@"Translit.zip"))
            {
                File.Replace(@"Translit.tmp", @"Translit.zip", null);
            }
            else
            {
                File.Move(@"Translit.tmp", @"Translit.zip");
            }

            Settings.Default.UpdateReady = true;
		}
	}
}
