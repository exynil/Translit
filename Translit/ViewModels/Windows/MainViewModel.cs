using MaterialDesignThemes.Wpf;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Translit.Models.Other;
using Translit.Models.Windows;
using Translit.Properties;
using Translit.Views.DialogWindows;
using Translit.Views.Pages;

namespace Translit.ViewModels.Windows
{
    class MainViewModel : INotifyPropertyChanged
	{
		private Page _currentPage;
		private string _currentPageName;
		private string _login;
		private string _userName;
		private GridLength _signInLength;
		private GridLength _logOutLength;
		private bool _canSignIn = true;
		private bool _canLogOut = true;
		public event PropertyChangedEventHandler PropertyChanged;
		public MainModel Model { get; set; }
		public Page FileConverterView { get; set; }
		public Page TextConverterView { get; set; }
		public Page SymbolsEditorView { get; set; }
		public Page WordsEditorView { get; set; }
		public Page SettingsView { get; set; }
		public Page DatabaseView { get; set; }
		public Page AboutView { get; set; }
		public Page LicenseView { get; set; }
		public Page FaqView { get; set; }

		public Page CurrentPage
		{
			get => _currentPage;
			set
			{
				_currentPage = value;
				OnPropertyChanged();
			}
		}

		public string CurrentPageName
		{
			get => _currentPageName;
			set
			{
				_currentPageName = value;
				OnPropertyChanged();
			}
		}

		public string Login
		{
			get => _login;
			set
			{
				_login = value;
				OnPropertyChanged();
			}
		}

		public string UserName
		{
			get => _userName;
			set
			{
			    _userName = value;
				OnPropertyChanged();
			}
		}

		public SnackbarMessageQueue MessageQueue { get; set; }

		public GridLength SignInLength
		{
			get => _signInLength;
			set
			{
				_signInLength = value;
				OnPropertyChanged();
			}
		}

		public GridLength LogOutLength
		{
			get => _logOutLength;
			set
			{
				_logOutLength = value;
				OnPropertyChanged();
			}
		}

		public bool CanSignIn
		{
			get => _canSignIn;
			set
			{
				_canSignIn = value;
				OnPropertyChanged();
			}
		}

		public bool CanLogOut
		{
			get => _canLogOut;
			set
			{
				_canLogOut = value;
				OnPropertyChanged();
			}
		}

		public MainViewModel()
		{
			Model = new MainModel();
		    MessageQueue = new SnackbarMessageQueue();

            if (OpenFileConverter.CanExecute(null))
			{
				OpenFileConverter.Execute(null);
			}

			UpdatePopupBox();

		    UserName = $"{User.LastName} {User.FirstName}";
		}
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private string GetRes(string key)
		{
			return Application.Current.Resources[key].ToString();
		}

		public ICommand OpenFileConverter
		{
			get
			{
				return new DelegateCommand(o =>
				{
					if (FileConverterView == null)
					{
						FileConverterView = new FileConverterView();
					}

					CurrentPage = FileConverterView;
					CurrentPageName = GetRes("MenuItemFileConverter");
				});
			}
		}

		public ICommand OpenTextConverter
		{
			get
			{
				return new DelegateCommand(o =>
				{
					if (TextConverterView == null)
					{
						TextConverterView = new TextConverterView();
					}

					CurrentPage = TextConverterView;
					CurrentPageName = GetRes("MenuItemTextConverter");
				});
			}
		}

		public ICommand OpenSymbols
		{
			get
			{
				return new DelegateCommand(o =>
				{
					if (SymbolsEditorView == null)
					{
						SymbolsEditorView = new SymbolsEditorView();
					}

					CurrentPage = SymbolsEditorView;
					CurrentPageName = GetRes("MenuItemSymbols");
				});
			}
		}

		public ICommand OpenWords
		{
			get
			{
				return new DelegateCommand(o =>
				{
					if (WordsEditorView == null)
					{
						WordsEditorView = new WordsEditorView();
					}

					CurrentPage = WordsEditorView;
					CurrentPageName = GetRes("MenuItemWords");
				});
			}
		}

		public ICommand OpenSettings
		{
			get
			{
				return new DelegateCommand(o =>
				{
					if (SettingsView == null)
					{
						SettingsView = new SettingsView();
					}

					CurrentPage = SettingsView;
					CurrentPageName = GetRes("MenuItemSettings");
				});
			}
		}

		public ICommand OpenDatabase
		{
			get
			{
				return new DelegateCommand(o =>
				{
					if (DatabaseView == null)
					{
						DatabaseView = new DatabaseView();
					}

					CurrentPage = DatabaseView;
					CurrentPageName = GetRes("MenuItemDatabase");
				});
			}
		}

		public ICommand OpenAbout
		{
			get
			{
				return new DelegateCommand(o =>
				{
					if (AboutView == null)
					{
						AboutView = new AboutView();
					}

					CurrentPage = AboutView;
					CurrentPageName = GetRes("MenuItemAbout");
				});
			}
		}

		public ICommand OpenLicense
		{
			get
			{
				return new DelegateCommand(o =>
				{
					if (LicenseView == null)
					{
						LicenseView = new LicenseView();
					}

					CurrentPage = LicenseView;
					CurrentPageName = GetRes("MenuItemLicense");
				});
			}
		}

		public ICommand OpenFaq
		{
			get
			{
				return new DelegateCommand(o =>
				{
					if (FaqView == null)
					{
						FaqView = new FaqView();
					}

					CurrentPage = FaqView;
					CurrentPageName = GetRes("MenuItemFaq");
				});
			}
		}

		public ICommand SetRussianLanguage
		{
			get
			{
				return new DelegateCommand(o =>
				{
				    if (App.Language.Name == "ru-RU") return;

					App.Language = Settings.Default.DefaultLanguage = new CultureInfo("ru-RU");

					try
					{
						Process.Start(@"..\TranslitLauncher.exe", "ru-RU");
					}
					catch (Exception)
					{
						// ignored
					}

				    // Просим пользователя перезагрузить программу
                    var questionView = new QuestionView(GetRes("ARebootOfTheProgramIsRequired"));

				    if (questionView.ShowDialog() != true) return;
				    System.Windows.Forms.Application.Restart();
				    Application.Current.Shutdown();
				});
			}
		}

		public ICommand SetEnglishLanguage
		{
			get
			{
				return new DelegateCommand(o =>
				{
				    if (App.Language.Name == "en-US") return;

                    App.Language = Settings.Default.DefaultLanguage = new CultureInfo("en-US");

					try
					{
						Process.Start(@"..\TranslitLauncher.exe", "en-US");
					}
					catch (Exception)
					{
						// ignored
					}

				    // Просим пользователя перезагрузить программу
                    var questionView = new QuestionView(GetRes("ARebootOfTheProgramIsRequired"));

				    if (questionView.ShowDialog() != true) return;
				    System.Windows.Forms.Application.Restart();
				    Application.Current.Shutdown();
                });
			}
		}

		public ICommand SetKazakhLanguage
		{
			get
			{
				return new DelegateCommand(o =>
				{
				    if (App.Language.Name == "kk-KZ") return;

                    App.Language = Settings.Default.DefaultLanguage = new CultureInfo("kk-KZ");

					try
					{
						Process.Start(@"..\TranslitLauncher.exe", "kk-KZ");
					}
					catch (Exception)
					{
						// ignored
					}

				    // Просим пользователя перезагрузить программу
				    var questionView = new QuestionView(GetRes("ARebootOfTheProgramIsRequired"));

				    if (questionView.ShowDialog() != true) return;
				    System.Windows.Forms.Application.Restart();
				    Application.Current.Shutdown();
                });
			}
		}

		public ICommand Exit
		{
			get
			{
				return new DelegateCommand(o =>
				{
				    ((Window) o).Close();
                    Settings.Default.Save();
				});
			}
		}

		public ICommand SignIn
		{
			get
			{
				return new DelegateCommand(o =>
				{
					var password = ((PasswordBox)o).Password;

					((PasswordBox) o).Password = "";

					Task.Factory.StartNew(() =>
					{						
						CanSignIn = false;

						if (Login != "" && password != "")
						{
							MessageQueue.Enqueue(GetRes("SnackBarAuthorization"));

						    var result = Model.SignIn(Login, password);

                            if (result == 0)
							{
							    MessageQueue.Enqueue(GetRes("SnackBarBadInternetConnection"));
							}
							else if (result == 1)
							{
							    MessageQueue.Enqueue(GetRes("SnackBarWrongLoginOrPassword"));
                            }
                            else
                            {
                                MessageQueue.Enqueue(GetRes("SnackBarWelcome"));

                                if (OpenFileConverter.CanExecute(null))
                                {
                                    OpenFileConverter.Execute(null);
                                }

                                if (SymbolsEditorView != null)
                                {
                                    SymbolsEditorView = null;
                                }

                                if (WordsEditorView != null)
                                {
                                    WordsEditorView = null;
                                }

                                UserName = $"{User.LastName} {User.FirstName}";
                                UpdatePopupBox();
                                Login = "";
                            }
						}

						CanSignIn = true;
                    });
				});
			}
		}

		public ICommand LogOut
		{
			get
			{
				return new DelegateCommand(o =>
				{
					Task.Factory.StartNew(() =>
					{
						CanLogOut = false;

						Model.LogOut();

                        if (OpenFileConverter.CanExecute(null))
						{
							OpenFileConverter.Execute(null);
							if (SymbolsEditorView != null)
							{
								SymbolsEditorView = null;
							}

							if (WordsEditorView != null)
							{
								WordsEditorView = null;
							}
						}

						UserName = "";
						UpdatePopupBox();
						CanLogOut = true;
                    });
				});
			}
		}

		public void UpdatePopupBox()
		{
			// Если пользователь авторизован
			if (Settings.Default.PermissionToChange)
			{
				SignInLength = new GridLength(0, GridUnitType.Pixel);
				LogOutLength = new GridLength(1, GridUnitType.Star);
			}
			else
			{
				SignInLength = new GridLength(1, GridUnitType.Star);
				LogOutLength = new GridLength(0, GridUnitType.Pixel);
			}
		}
	}
}
