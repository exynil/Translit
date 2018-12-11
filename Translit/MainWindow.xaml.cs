using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Translit.Entity;
using Translit.Pages;
using Translit.Properties;

namespace Translit
{
	public partial class MainWindow
	{
		private readonly FileConverterPage _fileConverterPage;
		private readonly TextConverterPage _textConverterPage;
		private readonly SymbolsEditorPage _symbolsEditorPage;
		private readonly SymbolsPage _symbolsPage;
		private readonly WordsEditorPage _wordsEditorPage;
		private readonly WordsPage _wordsPage;
		private readonly SettingsPage _settingsPage;
		private readonly DatabaseUpdatePage _databaseUpdatePage;
		private readonly AboutPage _aboutPage;

		private User _user;

		public MainWindow()
		{
			InitializeComponent();
			_fileConverterPage = new FileConverterPage(SnackbarMain);
			_textConverterPage = new TextConverterPage();
			_symbolsEditorPage = new SymbolsEditorPage(SnackbarMain);
			_symbolsPage = new SymbolsPage();
			_wordsEditorPage = new WordsEditorPage(SnackbarMain);
			_wordsPage = new WordsPage();
			_settingsPage = new SettingsPage();
			_databaseUpdatePage = new DatabaseUpdatePage(SnackbarMain);
			_aboutPage = new AboutPage(FrameMain);

			App.LanguageChanged += LanguageChanged;

			CultureInfo cultureInfo = App.Language;

			//Заполняем ComboBox списком языков
			foreach (var l in App.Languages)
			{
				string language = l.NativeName.Remove(l.NativeName.IndexOf("(", StringComparison.Ordinal),
					l.NativeName.Length - l.NativeName.IndexOf("(", StringComparison.Ordinal));

				ComboBoxItem comboBoxItem = new ComboBoxItem
				{
					Content = language.Substring(0, 1).ToUpper() + language.Remove(0, 1),
					Tag = l,
					IsSelected = l.Equals(cultureInfo)
				};
				comboBoxItem.Selected += ChangeLanguageClick;
				ComboBoxLanguages.Items.Add(comboBoxItem);
			}
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			// Получаем MAC адрес машины клиента
			string macAddress = NetworkInterface.GetAllNetworkInterfaces().Where(nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback).Select(nic => nic.GetPhysicalAddress().ToString()).FirstOrDefault();

			// Если MAC адрес текущей машины совпадает с MAC адресом последней машины
			if (macAddress == Settings.Default.MacAddress)
			{
				_user = new User();
				// Получаем объект из расшифрованного JSON
				_user = JsonConvert.DeserializeObject<User>(Rc4.Calc(macAddress, Settings.Default.User));
				// Обновляем данные пользователя в редакторах слов и символов
				_wordsEditorPage.User = _symbolsEditorPage.User = _user;
			}
			else
			{
				Settings.Default.User = "";
				Settings.Default.MacAddress = "";
			}

			// Обновляем заголовок окна
			TextBlockPageName.SetResourceReference(TextBlock.TextProperty, "ListViewItemFileConverter");
			// Загружаем в фрейми страницу файлового конвертора
			FrameMain.NavigationService.Navigate(_fileConverterPage);
			// Обновляем боковое меню
			UpdateRightMenu();
		}

		private void LanguageChanged(Object sender, EventArgs e)
		{
			CultureInfo currentLanguage = App.Language;

			//Отмечаем нужный пункт смены языка как выбранный язык
			foreach (ComboBoxItem i in ComboBoxLanguages.Items)
			{
				i.IsSelected = i.Tag is CultureInfo ci && ci.Equals(currentLanguage);
			}
			FrameMain.NavigationService.Refresh();
		}

		private void ChangeLanguageClick(Object sender, EventArgs e)
		{
			ComboBoxItem ci = sender as ComboBoxItem;
			if (ci?.Tag is CultureInfo lang)
			{
				App.Language = lang;
			}
		}
		

		private void ListViewMenu_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var items = ListViewMenu.Items;

			for (int i = 0; i < items.Count; i++)
			{
				if (i == ((ListView)sender).SelectedIndex)
				{
					((ListViewItem)items[i]).Foreground = new SolidColorBrush(Color.FromRgb(3, 169, 244));
					continue;
				}
				((ListViewItem)items[i]).Foreground = new SolidColorBrush(Color.FromRgb(64, 64, 64));
				
			}
			switch (((ListView)sender).SelectedIndex)
			{
				case 0:
				{
					TextBlockPageName.SetResourceReference(TextBlock.TextProperty, "ListViewItemFileConverter");
					FrameMain.NavigationService.Navigate(_fileConverterPage);
					break;
				}
				case 1:
				{
					TextBlockPageName.SetResourceReference(TextBlock.TextProperty, "ListViewItemTextConverter");
					FrameMain.NavigationService.Navigate(_textConverterPage);
					break;
				}
				case 2:
				{
					TextBlockPageName.SetResourceReference(TextBlock.TextProperty, "ListViewItemSymbols");
					if (Settings.Default.MacAddress != "")
					{
						FrameMain.NavigationService.Navigate(_symbolsEditorPage);
					}
					else
					{
						FrameMain.NavigationService.Navigate(_symbolsPage);
					}
					break;
				}
				case 3:
				{
					TextBlockPageName.SetResourceReference(TextBlock.TextProperty, "ListViewItemWords");
					if (Settings.Default.MacAddress != "")
					{
						FrameMain.NavigationService.Navigate(_wordsEditorPage);
					}
					else
					{
						FrameMain.NavigationService.Navigate(_wordsPage);
					}
					break;
				}
				case 4:
				{
					TextBlockPageName.SetResourceReference(TextBlock.TextProperty, "ListViewItemSettings");
					FrameMain.NavigationService.Navigate(_settingsPage);
					break;
				}
				case 5:
				{
					TextBlockPageName.SetResourceReference(TextBlock.TextProperty, "ListViewItemDatabase");
					FrameMain.NavigationService.Navigate(_databaseUpdatePage);
					break;
				}
				case 6:
				{
					TextBlockPageName.SetResourceReference(TextBlock.TextProperty, "ListViewItemAbout");
					FrameMain.NavigationService.Navigate(_aboutPage);
					break;
				}
			}
		}

		// Нажатие кнопки авторизации
		private void ButtonSignIn_OnClick(object sender, RoutedEventArgs e)
		{
			// Если поля логина и пароля не пусты
			if (TextBoxLogin.Text != "" && PasswordBoxPassword.Password != "")
			{
				// Выводим уведомление "Авторизация..."
				Task.Factory.StartNew(() => { })
					.ContinueWith(t =>
					{
						SnackbarMain.MessageQueue.Enqueue(Application.Current.Resources["SnackBarAuthorization"]);

					}, TaskScheduler.FromCurrentSynchronizationContext());
				// Получаем логин
				string login = TextBoxLogin.Text;
				// Получаем пароль
				string password = PasswordBoxPassword.Password;
				// Генерируем ссылку
				string link = "http://account.osmium.kz/api/auth?login=" + login + "&pass=" + password;
				// Создаем запрос
				HttpWebRequest request = (HttpWebRequest)WebRequest.Create(link);

				HttpWebResponse response;

				try
				{
					// Запрашиваем ответ от сервера
					response = (HttpWebResponse)request.GetResponse();
				}
				catch (Exception)
				{
					return;
				}
				
				// Считываем данные
				using (StreamReader stream = new StreamReader(response.GetResponseStream() ?? throw new InvalidOperationException(), Encoding.UTF8))
				{
					// Получаем объект из JSON
					_user = JsonConvert.DeserializeObject<User>(stream.ReadToEnd());
				}

				if (_user != null)
				{
					// Выводим уведомление "Добро пожаловать [фамилия] [имя]"
					Task.Factory.StartNew(() => { }).ContinueWith(t => { SnackbarMain.MessageQueue.Enqueue(Application.Current.Resources["SnackBarWelcome"] + " " + _user.LastName + " " + _user.FirstName); }, TaskScheduler.FromCurrentSynchronizationContext());
					// Получаем MAC адрес машины клиента
					string macAddress = NetworkInterface.GetAllNetworkInterfaces().Where(nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback).Select(nic => nic.GetPhysicalAddress().ToString()).FirstOrDefault();
					// Переводим нашего пользователя в строку JSON
					string userJson = JsonConvert.SerializeObject(_user);
					// Сохраняем зашифрованного пользователя в настройках приложения
					Settings.Default.User = Rc4.Calc(macAddress, userJson);
					// Сохраняем MAC адресс
					Settings.Default.MacAddress = macAddress;
					// Удлаляем логин и пароль из окна авторизации
					TextBoxLogin.Text = PasswordBoxPassword.Password = "";
					// Обновляем данные пользователя в редакторах слов и символов
					_wordsEditorPage.User = _symbolsEditorPage.User = _user;
					// Обновляем информацию об авторизовавшемся пользователе в правом боковом меню
					TextBlockUser.Text = _user.LastName + " " + _user.FirstName;
					// Обнолвяем боковое меню
					UpdateRightMenu();

					if (FrameMain.NavigationService.Content is WordsPage)
					{
						FrameMain.NavigationService.Navigate(_wordsEditorPage);
					}
					else if (FrameMain.NavigationService.Content is SymbolsPage)
					{
						FrameMain.NavigationService.Navigate(_symbolsEditorPage);
					}
				}
				else
				{
					// Выводим уведомление "Неверный логин или пароль"
					Task.Factory.StartNew(() => { }).ContinueWith(t => { SnackbarMain.MessageQueue.Enqueue(Application.Current.Resources["SnackBarWrongLoginOrPassword"]); }, TaskScheduler.FromCurrentSynchronizationContext());
				}
			}
		}

		// Обновление бокового меню
		private void UpdateRightMenu()
		{
			// Если MAC адрес пустой раскрываем окно авторизации, иначе раскрываем окно профиля
			if (Settings.Default.MacAddress == "")
			{
				RowDefinitionSignIn.Height = new GridLength(1, GridUnitType.Star);
				RowDefinitionLogOut.Height = new GridLength(0, GridUnitType.Star);
			}
			else
			{
				RowDefinitionSignIn.Height = new GridLength(0, GridUnitType.Star);
				RowDefinitionLogOut.Height = new GridLength(1, GridUnitType.Star);
				TextBlockUser.Text = _user.LastName + " " + _user.FirstName;
			}
		}

		// Нажатие кнопки выхода из профиля
		private void ButtonLogOut_OnClick(object sender, RoutedEventArgs e)
		{
			string link = "http://account.osmium.kz/api/auth?token=" + _user.Token + "&id=" + _user.Id;
			Debug.WriteLine(link);
			Dispatcher.InvokeAsync(() =>
			{
				HttpClient client = new HttpClient();

				client.DeleteAsync(link);

				// Обнуляем текущего пользователя
				_user = null;
				// Удаляем профиль в настройках приложения
				Settings.Default.User = "";
				// Удаляем MAC адресс в настройках приложения
				Settings.Default.MacAddress = "";
				// Обновляем боковое меню
				UpdateRightMenu();
			});
		}

		// Обработка нажатия клавиш
		//private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		//{
		//	switch (e.Key)
		//	{
		//		case System.Windows.Input.Key.F12:
		//		{
		//			LoadSymbols();
		//			break;
		//		}
		//	}
		//}

		// Метод для разработчика
		//private void LoadSymbols()
		//{
		//	string[] cyryllic = {
		//		"A", "а", "Ә", "ә", "Б", "б", "Д", "д", "E", "e", "Ф", "ф", "Г", "г",
		//		"Ғ", "ғ", "Х", "х", "И", "и", "Й", "й", "Ж", "ж", "К", "к", "Л", "л",
		//		"М", "м", "Н", "н", "Ң", "ң", "О", "о", "Ө", "ө", "П", "п", "Қ", "қ",
		//		"Р", "р", "С", "с", "Ш", "ш", "Ч", "ч", "Т", "т", "Ұ", "ұ", "Ү", "ү",
		//		"В", "в", "Ы", "ы", "У", "у", "З", "з", "Я", "я", "Ц", "ц", "Ю", "ю",
		//		"Щ", "щ", "Ъ", "ъ", "Ь", "ь", "Ё", "ё", "Э", "э"
		//	};
		//	string[] latin = {
		//		"A", "a", "Á", "á", "B", "b", "D", "d", "E", "e", "F", "f", "G", "g",
		//		"Ǵ", "ǵ", "H", "h", "І", "і", "Í", "í", "J", "j", "K", "k", "L", "l",
		//		"M", "m", "N", "n", "Ń", "ń", "O", "o", "Ó", "ó", "P", "p", "Q", "q",
		//		"R", "r", "S", "s", "Sh", "sh", "Ch", "ch", "T", "t", "U", "u", "Ú",
		//		"ú", "V", "v", "Y", "y", "Ý", "ý", "Z", "z", "Ia", "ıa", "S", "s",
		//		"Iý", "ıý", "Sh", "sh", "", "", "", "", "Io", "ıo","E", "e"
		//	};


		//	//for (int i = 0; i < cyryllic.Length; i++)
		//	//{
		//	//	Debug.WriteLine("INSERT INTO `v464un7p80gkr068`.`symbol` (`cyrl`, `latn`) VALUES('" + cyryllic[i] + "', '" + latin[i] + "');");
		//	//}

		//	File.Delete(@".\Database\localdb.db");

		//	using (LiteDatabase db = new LiteDatabase(@".\Database\localdb.db"))
		//	{
		//		var symbols = db.GetCollection<Symbol>("Symbols");

		//		for (int i = 0; i < cyryllic.Length; i++)
		//		{
		//			symbols.Insert(new Symbol
		//			{
		//				Cyryllic = cyryllic[i],
		//				Latin = latin[i]
		//			});
		//		}
		//	}
		//}
	}
}
