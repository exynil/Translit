using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using Newtonsoft.Json;
using Translit.Entity;
using Translit.Properties;

namespace Translit.Models.Windows
{
	public class MainWindowModel : IMainWindowModel
	{
		public bool SignIn(string login, string password)
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
			var link = "http://account.osmium.kz/api/auth?token=" + user.Token + "&id=" + user.Id;
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
	}
}
