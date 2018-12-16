using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using Translit.Entity;
using Translit.Properties;

namespace Translit.Models.Windows
{
	public class MainWindowModel
	{
		// Свойства
		public string Login { get; set; }
		public string Password { get; set; }

		// Методы
		public bool IsLoginOrPasswordEmpty()
		{
			return Login == "" || Password == "";
		}

		public User SignIn()
		{
			User user;

			var link = "http://account.osmium.kz/api/auth?login=" + Login + "&pass=" + Password;

			var request = (HttpWebRequest) WebRequest.Create(link);

			HttpWebResponse response;

			try
			{
				// Запрашиваем ответ от сервера
				response = (HttpWebResponse) request.GetResponse();
			}
			catch (Exception)
			{
				return null;
			}

			var responseStream = response.GetResponseStream();

			// Считываем данные
			using (var stream = new StreamReader(responseStream ?? throw new InvalidOperationException(), Encoding.UTF8))
			{
				// Получаем объект из JSON
				user = JsonConvert.DeserializeObject<User>(stream.ReadToEnd());
			}

			SaveMacAddressAndUser(user);
			return user;
		}

		public void SaveMacAddressAndUser(User user)
		{
			var macAddress = NetworkInterface.GetAllNetworkInterfaces()
				.Where(nic => nic.OperationalStatus == OperationalStatus.Up &&
				              nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
				.Select(nic => nic.GetPhysicalAddress()
					.ToString())
				.FirstOrDefault();
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
			var macAddress = NetworkInterface.GetAllNetworkInterfaces()
				.Where(nic => nic.OperationalStatus == OperationalStatus.Up &&
				              nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
				.Select(nic => nic.GetPhysicalAddress().ToString()).FirstOrDefault();

			// Возвращяем расшифрованного и дисериализованного пользователя
			return JsonConvert.DeserializeObject<User>(Rc4.Calc(macAddress, Settings.Default.User));

		}

		public void DeleteToken()
		{
			var user = GetUser();
			var link = "http://account.osmium.kz/api/auth?token=" + user.Token + "&id=" + user.Id;
			var client = new HttpClient();
			client.DeleteAsync(link);
		}

		public void DeleteUserFromSettings()
		{
			// Удаляем профиль в настройках приложения
			Settings.Default.User = "";
			// Удаляем MAC адресс в настройках приложения
			Settings.Default.MacAddress = "";
		}

		// Сравнение сохраненного MAC адреса с MAC адресом текущей машины
		public void MacAddressComparison()
		{
			// Получаем MAC адрес машины клиента
			var macAddress = NetworkInterface.GetAllNetworkInterfaces()
				.Where(nic => nic.OperationalStatus == OperationalStatus.Up &&
				              nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
				.Select(nic => nic.GetPhysicalAddress()
					.ToString())
				.FirstOrDefault();

			// Если MAC адрес текущей машины совпадает с MAC адресом последней машины
			if (macAddress == Settings.Default.MacAddress) return;

			DeleteUserFromSettings();
		}
	}
}
