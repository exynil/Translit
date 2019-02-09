using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Translit.Entity;
using Translit.Properties;

namespace Translit.Models.Windows
{
	class MainModel : IMainModel
	{
		public MainModel()
		{
			Task.Factory.StartNew(() => { CheckAndDownloadUpdate(); });
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

				Settings.Default.IsUserAuthorized = true;
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
			Settings.Default.IsUserAuthorized = false;
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

		private static string GetMacAddress()
		{
			var interfaces = NetworkInterface.GetAllNetworkInterfaces();
			var macAddress = interfaces
				.Where(x => x.OperationalStatus == OperationalStatus.Up && x.NetworkInterfaceType != NetworkInterfaceType.Loopback)
				.Select(x => x.GetPhysicalAddress())
				.FirstOrDefault()
				?.ToString();
			return macAddress;
		}

		private void CheckAndDownloadUpdate()
		{
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11;
			const string api = @"https://api.github.com/repos/OsmiumKZ/Translit/releases/latest";
			GithubResponse response;

			var client = new WebClient();

			client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

			try
			{
				var stream = client.OpenRead(api);
				var streamReader = new StreamReader(stream ?? throw new InvalidOperationException());

				response = JsonConvert.DeserializeObject<GithubResponse>(streamReader.ReadToEnd());

				stream.Close();
				streamReader.Close();
			}
			catch (Exception)
			{
				return;
			}

			var version = Assembly.GetExecutingAssembly().GetName().Version;

			var currentVersion = int.Parse($"{ version.Major}{ version.Minor}");
			var newVersion = int.Parse(response.TagName.Substring(1).Replace(".", ""));

			if (currentVersion >= newVersion) return;

			Settings.Default.UpdateReady = false;

			if (!Directory.Exists("Update"))
			{
				Directory.CreateDirectory("Update");
			}

			try
			{
				ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
				var webClient = new WebClient();
				webClient.DownloadFileCompleted += Completed;
				webClient.DownloadFileAsync(response.Assets[0].BrowserDownloadUrl, @"Update\Translit.zip");
			}
			catch (Exception)
			{
				// ignored
			}
		}

		private void Completed(object sender, AsyncCompletedEventArgs e)
		{
			Settings.Default.UpdateReady = true;
		}
	}
}
