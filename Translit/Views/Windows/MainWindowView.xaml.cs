using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
		public Page FileConverterView { get; }
		public Page TextConverterView { get; }
		public Page SymbolsEditorView { get; }
		public Page SymbolsView { get; }
		public Page WordsEditorView { get; }
		public Page WordsView { get; }
		public Page SettingsView { get; }
		public Page DatabaseView { get; }
		public Page AboutView { get; }

		public MainWindowView()
		{
			InitializeComponent();
			Presenter = new MainWindowPresenter(this);

			FileConverterView = new FileConverterView();
			TextConverterView = new TextConverterView();
			SymbolsEditorView = new SymbolsEditorView();
			SymbolsView = new SymbolsView();
			WordsEditorView = new WordsEditorView();
			WordsView = new WordsView();
			SettingsView = new SettingsView();
			DatabaseView = new DatabaseView();
			AboutView = new AboutView();

			App.LanguageChanged += LanguageChanged;
		}

		public void ButtonSignIn_OnClick(object sender, RoutedEventArgs e)
		{
			Presenter.OnButtonSignInClicked();
		}
		public void Window_Loaded(object sender, RoutedEventArgs e)
		{
			Presenter.OnWindowLoaded();

			//Заполняем ComboBox списком языков
			foreach (var l in App.Languages)
			{
				var language = l.NativeName.Remove(l.NativeName.IndexOf("(", StringComparison.Ordinal),
					l.NativeName.Length - l.NativeName.IndexOf("(", StringComparison.Ordinal));
				var comboBoxItem = new ComboBoxItem
				{
					Content = language.Substring(0, 1).ToUpper() + language.Remove(0, 1),
					Tag = l,
					IsSelected = l.Equals(App.Language)
				};
				comboBoxItem.Selected += ChangeLanguageClick;
				ComboBoxLanguages.Items.Add(comboBoxItem);
			}

			TextBoxLogin.Text = "exynil";
			PasswordBoxPassword.Password = "ssd3141593";
		}

		public void LanguageChanged(object sender, EventArgs e)
		{
			Presenter.OnLanguageChanged();
		}

		public void ChangeLanguageClick(object sender, EventArgs e)
		{
			var ci = sender as ComboBoxItem;
			if (ci?.Tag is CultureInfo lang)
			{
				App.Language = lang;
			}
		}

		public void ListViewMenu_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			Presenter.OnListViewMenuSelectionChanged(((ListView)sender).SelectedIndex);
		}

		public void ChangeListViewItemColor()
		{
			var items = ListViewMenu.Items;
			for (var i = 0; i < items.Count; i++)
			{
				if (i == ListViewMenu.SelectedIndex)
				{
					((ListViewItem)items[i]).Foreground = new SolidColorBrush(Color.FromRgb(3, 169, 244));
					continue;
				}

				((ListViewItem)items[i]).Foreground = new SolidColorBrush(Color.FromRgb(64, 64, 64));
			}
		}

		// Нажатие кнопки выхода из профиля
		public void ButtonLogOut_OnClick(object sender, RoutedEventArgs e)
		{
			Presenter.LogOut();
		}

		// Обновление бокового меню
		public void UpdateRightMenu(User user)
		{
			// Если MAC адрес пустой раскрываем окно авторизации, иначе раскрываем окно профиля
			if (user == null)
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

				TextBlockUser.Dispatcher.Invoke(() => TextBlockUser.Text = user.LastName + " " + user.FirstName,
					DispatcherPriority.Background);
			}
		}

		public string GetRes(string key)
		{
			return Application.Current.Resources[key].ToString();
		}

		private void TextBoxLogin_OnTextChanged(object sender, TextChangedEventArgs e)
		{
			Presenter.OnTextBlockLoginChanged(TextBoxLogin.Text);
		}

		private void PasswordBoxPassword_OnPasswordChanged(object sender, RoutedEventArgs e)
		{
			Presenter.OnPasswordBoxPasswordChanged(PasswordBoxPassword.Password);
		}

		public void ShowNotification(string resource)
		{
			SnackbarMain.Dispatcher.Invoke(() =>
			{
				SnackbarMain.MessageQueue.Enqueue(GetRes(resource));
			}, DispatcherPriority.Background);
		}

		public void ClearAuthorizationForm()
		{
			TextBoxLogin.Dispatcher.Invoke(() => { TextBoxLogin.Text = ""; },
				DispatcherPriority.Background);
			PasswordBoxPassword.Dispatcher.Invoke(() => { PasswordBoxPassword.Password = ""; }, DispatcherPriority.Background);
		}

		public void SetTitle(int number)
		{
			switch (number)
			{
				case 0:
					TextBlockPageName.SetResourceReference(TextBlock.TextProperty, "ListViewItemFileConverter");
					break;
				case 1:
					TextBlockPageName.SetResourceReference(TextBlock.TextProperty, "ListViewItemTextConverter");
					break;
				case 2:
					TextBlockPageName.SetResourceReference(TextBlock.TextProperty, "ListViewItemSymbols");
					break;
				case 3:
					TextBlockPageName.SetResourceReference(TextBlock.TextProperty, "ListViewItemWords");
					break;
				case 4:
					TextBlockPageName.SetResourceReference(TextBlock.TextProperty, "ListViewItemSettings");
					break;
				case 5:
					TextBlockPageName.SetResourceReference(TextBlock.TextProperty, "ListViewItemDatabase");
					break;
				case 6:
					TextBlockPageName.SetResourceReference(TextBlock.TextProperty, "ListViewItemAbout");
					break;
			}
		}

		public void ChangePage(int number)
		{
			switch (number)
			{
				case 0:
					FrameMain.Content = FileConverterView;
					break;
				case 1:
					FrameMain.Content = TextConverterView;
					break;
				case 2:
					FrameMain.Content = Settings.Default.MacAddress == "" ? SymbolsView : SymbolsEditorView;
					break;
				case 3:
					FrameMain.Content = Settings.Default.MacAddress == "" ? WordsView : WordsEditorView;
					break;
				case 4:
					FrameMain.Content = SettingsView;
					break;
				case 5:
					FrameMain.Content = DatabaseView;
					break;
				case 6:
					FrameMain.Content = AboutView;
					break;
			}
		}

		public void SelectLanguage()
		{
			foreach (ComboBoxItem i in ComboBoxLanguages.Items)
			{
				i.IsSelected = i.Tag is CultureInfo ci && ci.Equals(App.Language);
			}
		}

		public void BlockUnlockButtons()
		{
			ButtonSignIn.IsEnabled = !ButtonSignIn.IsEnabled;
			ButtonLogOut.IsEnabled = !ButtonLogOut.IsEnabled;
		}

		public void RefreshFrame()
		{
			FrameMain.Dispatcher.Invoke(() => { FrameMain.NavigationService.Refresh(); }, DispatcherPriority.Background);
		}
	}
}
