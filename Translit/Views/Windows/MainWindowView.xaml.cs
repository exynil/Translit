using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Translit.Entity;
using Translit.Presenters.Windows;
using Translit.Properties;
using Translit.Views.Pages;

namespace Translit.Views.Windows
{
	public partial class MainWindowView : IMainWindowView
	{
		private IMainWindowPresenter Presenter { get; }
		public Page FileConverterView { get; set; }
		public Page TextConverterView { get; set; }
		public Page SymbolsEditorView { get; set; }
		public Page SymbolsView { get; set; }
		public Page WordsEditorView { get; set; }
		public Page WordsView { get; set; }
		public Page SettingsView { get; set; }
		public Page DatabaseView { get; set; }
		public Page AboutView { get; set; }
		public Page LicenseView { get; set; }
		public Page FaqView { get; set; }

		public MainWindowView()
		{
			InitializeComponent();
			Presenter = new MainWindowPresenter(this);
		}

		public void Window_Loaded(object sender, RoutedEventArgs e)
		{
			Presenter.OnWindowLoaded();

			if (FileConverterView == null)
			{
				FileConverterView = new FileConverterView();
			}

			FrameTranslit.Content = FileConverterView;
			TextBlockCurrentPageName.SetResourceReference(TextBlock.TextProperty, "MenuItemFileConverter");

			Loaded -= Window_Loaded;
		}

		// Нажатие кнопки авторизации
		public void ButtonSignIn_OnClick(object sender, RoutedEventArgs e)
		{
			if (TextBoxLogin.Text != "" && PasswordBoxPassword.Password != "")
			{
				Presenter.OnButtonSignInClicked(TextBoxLogin.Text, PasswordBoxPassword.Password);
			}
		}

		// Нажатие кнопки выхода из профиля
		public void ButtonLogOut_OnClick(object sender, RoutedEventArgs e)
		{
			Presenter.LogOut();
		}

		// Обновление бокового меню
		public void UpdatePopupBox()
		{
			// Если пользователь авторизован
			if (Settings.Default.IsUserAuthorized)
			{
				RowDefinitionSignIn.Dispatcher.Invoke(() => RowDefinitionSignIn.Height = new GridLength(0, GridUnitType.Pixel),
					DispatcherPriority.Background);
				RowDefinitionLogOut.Dispatcher.Invoke(() => RowDefinitionLogOut.Height = new GridLength(1, GridUnitType.Star),
					DispatcherPriority.Background);
			}
			else
			{
				RowDefinitionSignIn.Dispatcher.Invoke(() => RowDefinitionSignIn.Height = new GridLength(1, GridUnitType.Star),
					DispatcherPriority.Background);
				RowDefinitionLogOut.Dispatcher.Invoke(() => RowDefinitionLogOut.Height = new GridLength(0, GridUnitType.Pixel),
					DispatcherPriority.Background);
			}
		}

		public string GetRes(string key)
		{
			return Application.Current.Resources[key].ToString();
		}

		public void ShowNotification(string key)
		{
			SnackbarNotification.Dispatcher.Invoke(() =>
			{
				SnackbarNotification.MessageQueue.Enqueue(GetRes(key));
			}, DispatcherPriority.Background);
		}

		public void ClearAuthorizationForm()
		{
			TextBoxLogin.Dispatcher.Invoke(() => { TextBoxLogin.Text = ""; },
				DispatcherPriority.Background);
			PasswordBoxPassword.Dispatcher.Invoke(() => { PasswordBoxPassword.Password = ""; }, DispatcherPriority.Background);
		}

		public void BlockUnlockButtons()
		{
			ButtonSignIn.IsEnabled = !ButtonSignIn.IsEnabled;
			ButtonLogOut.IsEnabled = !ButtonLogOut.IsEnabled;
		}

		public void ReloadFrame()
		{
			FrameTranslit.Dispatcher.Invoke(() => { FrameTranslit.Content = FileConverterView; }, DispatcherPriority.Background);
		}

		public void SetUserName(User user)
		{
			TextBlockUser.Dispatcher.Invoke(() =>
			{
				TextBlockUser.Text = user.LastName + " " + user.FirstName;
			}, DispatcherPriority.Background);
		}

		public void ClearUserName()
		{
			TextBlockUser.Dispatcher.Invoke(() =>
			{
				TextBlockUser.Text = "";
			}, DispatcherPriority.Background);
		}

		private void FileConverterMenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			if (FileConverterView == null)
			{
				FileConverterView = new FileConverterView();
			}

			FrameTranslit.Content = FileConverterView;
			TextBlockCurrentPageName.SetResourceReference(TextBlock.TextProperty, "MenuItemFileConverter");
		}

		private void TextConverterMenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			if (TextConverterView == null)
			{
				TextConverterView = new TextConverterView();
			}
			
			FrameTranslit.Content = new TextConverterView();
			TextBlockCurrentPageName.SetResourceReference(TextBlock.TextProperty, "MenuItemTextConverter");
		}

		private void ExitMenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void SymbolsMenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			if (Settings.Default.MacAddress == "")
			{
				if (SymbolsView == null)
				{
					SymbolsView = new SymbolsView();
				}

				FrameTranslit.Content = SymbolsView;
			}
			else
			{
				if (SymbolsEditorView == null)
				{
					SymbolsEditorView = new SymbolsEditorView();
				}

				FrameTranslit.Content = SymbolsEditorView;
			}

			TextBlockCurrentPageName.SetResourceReference(TextBlock.TextProperty, "MenuItemSymbols");
		}

		private void WordsMenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			if (Settings.Default.MacAddress == "")
			{
				if (WordsView == null)
				{
					WordsView = new WordsView();
				}

				FrameTranslit.Content = WordsView;
			}
			else
			{
				if (WordsEditorView == null)
				{
					WordsEditorView = new WordsEditorView();
				}

				FrameTranslit.Content = WordsEditorView;
			}
			TextBlockCurrentPageName.SetResourceReference(TextBlock.TextProperty, "MenuItemWords");
		}

		private void SettingsMenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			if (SettingsView == null)
			{
				SettingsView = new SettingsView();
			}

			FrameTranslit.Content = new SettingsView();
			TextBlockCurrentPageName.SetResourceReference(TextBlock.TextProperty, "MenuItemSettings");
		}

		private void DatabaseMenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			if (DatabaseView == null)
			{
				DatabaseView = new DatabaseView();
			}

			FrameTranslit.Content = DatabaseView;
			TextBlockCurrentPageName.SetResourceReference(TextBlock.TextProperty, "MenuItemDatabase");
		}

		private void AboutMenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			if (AboutView == null)
			{
				AboutView = new AboutView();
			}

			FrameTranslit.Content = AboutView;
			TextBlockCurrentPageName.SetResourceReference(TextBlock.TextProperty, "MenuItemAbout");
		}

		private void LicenseMenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			if (LicenseView == null)
			{
				LicenseView = new LicenseView();
			}

			FrameTranslit.Content = LicenseView;
			TextBlockCurrentPageName.SetResourceReference(TextBlock.TextProperty, "MenuItemLicense");
		}

		private void FaqMenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			if (FaqView == null)
			{
				FaqView = new FaqView();
			}

			FrameTranslit.Content = FaqView;
			TextBlockCurrentPageName.Text = "FAQ";
		}

		private void RussianMenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			App.Language = Settings.Default.DefaultLanguage = new CultureInfo("ru-RU");

			try
			{
				Process.Start(@"..\TranslitLauncher.exe", "ru-RU");
			}
			catch (Exception)
			{
				// ignored
			}
			
		}

		private void EnglishMenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			App.Language = Settings.Default.DefaultLanguage = new CultureInfo("en-US");
			
			try
			{
				Process.Start(@"..\TranslitLauncher.exe", "en-US");
			}
			catch (Exception)
			{
				// ignored
			}
		}

		private void KazakhMenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			App.Language = Settings.Default.DefaultLanguage = new CultureInfo("kk-KZ");

			try
			{
				Process.Start(@"..\TranslitLauncher.exe", "kk-KZ");
			}
			catch (Exception)
			{
				// ignored
			}
		}
	}
}
