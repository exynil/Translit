using Newtonsoft.Json;
using System;
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
using System.Windows.Threading;
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
			var cultureInfo = App.Language;

			//Заполняем ComboBox списком языков
			foreach (var l in App.Languages)
			{
				var language = l.NativeName.Remove(l.NativeName.IndexOf("(", StringComparison.Ordinal),
					l.NativeName.Length - l.NativeName.IndexOf("(", StringComparison.Ordinal));
				var comboBoxItem = new ComboBoxItem
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
			var macAddress = NetworkInterface.GetAllNetworkInterfaces()
				.Where(nic => nic.OperationalStatus == OperationalStatus.Up &&
				              nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
				.Select(nic => nic.GetPhysicalAddress().ToString()).FirstOrDefault();

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

		private void LanguageChanged(object sender, EventArgs e)
		{
			var currentLanguage = App.Language;

			//Отмечаем нужный пункт смены языка как выбранный язык
			foreach (ComboBoxItem i in ComboBoxLanguages.Items)
			{
				i.IsSelected = i.Tag is CultureInfo ci && ci.Equals(currentLanguage);
			}

			FrameMain.NavigationService.Refresh();
		}

		private void ChangeLanguageClick(object sender, EventArgs e)
		{
			var ci = sender as ComboBoxItem;
			if (ci?.Tag is CultureInfo lang)
			{
				App.Language = lang;
			}
		}

		private void ListViewMenu_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var items = ListViewMenu.Items;
			for (var i = 0; i < items.Count; i++)
			{
				if (i == ((ListView) sender).SelectedIndex)
				{
					((ListViewItem) items[i]).Foreground = new SolidColorBrush(Color.FromRgb(3, 169, 244));
					continue;
				}

				((ListViewItem) items[i]).Foreground = new SolidColorBrush(Color.FromRgb(64, 64, 64));
			}

			switch (((ListView) sender).SelectedIndex)
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
		private async void ButtonSignIn_OnClick(object sender, RoutedEventArgs e)
		{
			// Если поля логина и пароля не пусты
			if (TextBoxLogin.Text == "" || PasswordBoxPassword.Password == "") return;

			// Выводим уведомление "Авторизация..."
			await Task.Factory.StartNew(() => { })
				.ContinueWith(t => { SnackbarMain.MessageQueue.Enqueue(GetRes("SnackBarAuthorization")); },
					TaskScheduler.FromCurrentSynchronizationContext());
			// Получаем логин
			var login = TextBoxLogin.Text;
			// Получаем пароль
			var password = PasswordBoxPassword.Password;
			// Генерируем ссылку
			var link = "http://account.osmium.kz/api/auth?login=" + login + "&pass=" + password;
			// Создаем запрос
			var request = (HttpWebRequest) WebRequest.Create(link);
			ButtonSignIn.IsEnabled = false;
			await Task.Factory.StartNew(() =>
			{
				HttpWebResponse response;
				try
				{
					// Запрашиваем ответ от сервера
					response = (HttpWebResponse) request.GetResponse();
				}
				catch (Exception)
				{
					return;
				}

				var responseStream = response.GetResponseStream();

				// Считываем данные
				using (var stream = new StreamReader(responseStream ?? throw new InvalidOperationException(), Encoding.UTF8))
				{
					// Получаем объект из JSON
					_user = JsonConvert.DeserializeObject<User>(stream.ReadToEnd());
				}

				if (_user != null)
				{
					// Выводим уведомление "Добро пожаловать [фамилия] [имя]"
					SnackbarMain.Dispatcher.Invoke(() =>
					{
						var text = GetRes("SnackBarWelcome") + " " + _user.LastName + " " + _user.FirstName;
						SnackbarMain.MessageQueue.Enqueue(text);
					}, DispatcherPriority.Background);
					// Получаем MAC адрес машины клиента
					var macAddress = NetworkInterface.GetAllNetworkInterfaces()
						.Where(nic => nic.OperationalStatus == OperationalStatus.Up &&
						              nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
						.Select(nic => nic.GetPhysicalAddress().ToString()).FirstOrDefault();
					// Переводим нашего пользователя в строку JSON
					var userJson = JsonConvert.SerializeObject(_user);
					// Сохраняем зашифрованного пользователя в настройках приложения
					Settings.Default.User = Rc4.Calc(macAddress, userJson);
					// Сохраняем MAC адресс
					Settings.Default.MacAddress = macAddress;
					// Удлаляем логин и пароль из окна авторизации
					TextBoxLogin.Dispatcher.Invoke(() => TextBoxLogin.Text = "", DispatcherPriority.Background);
					// Обновляем данные пользователя в редакторах слов и символов
					_wordsEditorPage.User = _symbolsEditorPage.User = _user;
					// Обновляем информацию об авторизовавшемся пользователе в правом боковом меню
					TextBlockUser.Dispatcher.Invoke(() => TextBlockUser.Text = _user.LastName + " " + _user.FirstName,
						DispatcherPriority.Background);
					// Обнолвяем боковое меню
					UpdateRightMenu();
					FrameMain.Dispatcher.Invoke(() => { FrameMain.NavigationService.Navigate(_fileConverterPage); },
						DispatcherPriority.Background);
				}
				else
				{
					// Выводим уведомление "Неверный логин или пароль"
					SnackbarMain.Dispatcher.Invoke(
						() => { SnackbarMain.MessageQueue.Enqueue(GetRes("SnackBarWrongLoginOrPassword")); },
						DispatcherPriority.Background);
				}
			});
			ButtonSignIn.IsEnabled = true;
		}

		// Нажатие кнопки выхода из профиля
		private async void ButtonLogOut_OnClick(object sender, RoutedEventArgs e)
		{
			var link = "http://account.osmium.kz/api/auth?token=" + _user.Token + "&id=" + _user.Id;
			ButtonLogOut.IsEnabled = false;
			await Task.Factory.StartNew(() =>
			{
				var client = new HttpClient();
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
			ButtonLogOut.IsEnabled = true;
		}

		// Обновление бокового меню
		private void UpdateRightMenu()
		{
			// Если MAC адрес пустой раскрываем окно авторизации, иначе раскрываем окно профиля
			if (Settings.Default.MacAddress == "")
			{
				RowDefinitionSignIn.Dispatcher.Invoke(() => RowDefinitionSignIn.Height = new GridLength(1, GridUnitType.Star),
					DispatcherPriority.Background);
				RowDefinitionLogOut.Dispatcher.Invoke(() => RowDefinitionLogOut.Height = new GridLength(0, GridUnitType.Star),
					DispatcherPriority.Background);
			}
			else
			{
				RowDefinitionSignIn.Dispatcher.Invoke(() => RowDefinitionSignIn.Height = new GridLength(0, GridUnitType.Star),
					DispatcherPriority.Background);
				RowDefinitionLogOut.Dispatcher.Invoke(() => RowDefinitionLogOut.Height = new GridLength(1, GridUnitType.Star),
					DispatcherPriority.Background);
				TextBlockUser.Dispatcher.Invoke(() => TextBlockUser.Text = _user.LastName + " " + _user.FirstName,
					DispatcherPriority.Background);
			}
		}

		private string GetRes(string key)
		{
			return Application.Current.Resources[key].ToString();
		}
	}
}
