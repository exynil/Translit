using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;
using Translit.Entity;
using Translit.Models.Pages;

namespace Translit.ViewModels.Pages
{
	class DatabaseViewModel : INotifyPropertyChanged
	{
		private string _database;
		private string _databaseSize;
		private string _symbolsCount;
		private string _wordsCount;
		private string _lastUpdate;
		private Visibility _progressBarVisibility;
		private string _symbolsInserting;
		private int _percentOfSymbols;
		private string _wordsInserting;
		private int _percentOfWords;
		private bool _canUpdate;
		private bool _isIndeterminate;
		private string _updateButtonContent;
		public event PropertyChangedEventHandler PropertyChanged;
		public DatabaseModel Model { get; set; }
		public SnackbarMessageQueue MessageQueue { get; set; }
		public string Database
		{
			get => _database;
			set
			{
				_database = value;
				OnPropertyChanged();
			}
		}

		public string DatabaseSize
		{
			get => _databaseSize;
			set
			{
				_databaseSize = value;
				OnPropertyChanged();
			}
		}

		public string SymbolsCount
		{
			get => _symbolsCount;
			set
			{
				_symbolsCount = value;
				OnPropertyChanged();
			}
		}

		public string WordsCount
		{
			get => _wordsCount;
			set
			{
				_wordsCount = value;
				OnPropertyChanged();
			}
		}

		public string LastUpdate
		{
			get => _lastUpdate;
			set
			{
				_lastUpdate = value;
				OnPropertyChanged();
			}
		}

		public Visibility ProgressBarVisibility
		{
			get => _progressBarVisibility;
			set
			{
				_progressBarVisibility = value;
				OnPropertyChanged();
			}
		}

		public string SymbolsInserting
		{
			get => _symbolsInserting;
			set
			{
				_symbolsInserting = value;
				OnPropertyChanged();
			}
		}

		public int PercentOfSymbols
		{
			get => _percentOfSymbols;
			set
			{
				_percentOfSymbols = value;
				OnPropertyChanged();
			}
		}

		public string WordsInserting
		{
			get => _wordsInserting;
			set
			{
				_wordsInserting = value;
				OnPropertyChanged();
			}
		}

		public int PercentOfWords
		{
			get => _percentOfWords;
			set
			{
				_percentOfWords = value;
				OnPropertyChanged();
			}
		}

		public bool CanUpdate
		{
			get => _canUpdate;
			set
			{
				_canUpdate = value;
				OnPropertyChanged();
			}
		}

		public bool IsIndeterminate
		{
			get => _isIndeterminate;
			set
			{
				_isIndeterminate = value;
				OnPropertyChanged();
			}
		}

		public string UpdateButtonContent
		{
			get => _updateButtonContent;
			set
			{
				_updateButtonContent = value;
				OnPropertyChanged();
			}
		}

		public DatabaseViewModel()
		{
			Model = new DatabaseModel();
			Model.PropertyChanged += OnModelPropertyChanged;
			MessageQueue = new SnackbarMessageQueue();
			ProgressBarVisibility = Visibility.Hidden;
			CanUpdate = true;
			IsIndeterminate = false;
			UpdateButtonContent = GetRes("ButtonUpdate");

			SetInfoAboutDatabase();
		}

		private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			PercentOfSymbols = Model.PercentOfSymbols;
			PercentOfWords = Model.PercentOfWords;
			SymbolsInserting = GetRes("TextBlockCharacterLoading") + ": " + PercentOfSymbols + "%";
			WordsInserting = GetRes("TextBlockLoadingWords") + ": " + PercentOfWords + "%";
		}

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public ICommand Update
		{
			get
			{
				return new DelegateCommand(o =>
				{
					Task.Factory.StartNew(() =>
					{
						CanUpdate = false;
						IsIndeterminate = true;
						UpdateButtonContent = GetRes("ButtonLoading");
						if (!Model.DownloadDatabaseFromServer())
						{
							IsIndeterminate = false;
							CanUpdate = true;
							UpdateButtonContent = GetRes("ButtonUpdate");
							MessageQueue.Enqueue(GetRes("SnackBarBadInternetConnection"));
							return;
						}
						ProgressBarVisibility = Visibility.Visible;
						Model.DeleteOldDatabase();
						Model.InsertData();
						MessageQueue.Enqueue(GetRes("SnackBarUpdateCompleted"));
						SetInfoAboutDatabase();
						ProgressBarVisibility = Visibility.Hidden;
						UpdateButtonContent = GetRes("ButtonUpdate");
						IsIndeterminate = false;
						CanUpdate = true;
					});
				});
			}
		}

		private string GetRes(string key)
		{
			return Application.Current.Resources[key].ToString();
		}

		private void SetInfoAboutDatabase()
		{
			if (Model.DatabaseExists())
			{
				Database = GetRes("TextBlockDatabase");
				DatabaseSize = $"{GetRes("TextBlockDatabaseSize")} {Model.GetDatabaseSize() / 1024} KB";
				SymbolsCount = $"{GetRes("TextBlockAmountOfCharacters")} {Model.GetSymbolCount()}";
				WordsCount = $"{GetRes("TextBlockAmountOfWords")} {Model.GetWordCount()}";
				LastUpdate = $"{GetRes("TextBlockLastUpdate")} {Model.GetLastWriteTime().ToString(CultureInfo.CurrentCulture)}";
			}
			else
			{
				Database = GetRes("TextBlockDatabaseNotFound");
			}
		}
	}
}
