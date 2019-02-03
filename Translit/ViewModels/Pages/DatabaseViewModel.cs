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
		public event PropertyChangedEventHandler PropertyChanged;
		public DatabaseModel Model { get; set; }
		public SnackbarMessageQueue MessageQueue { get; set; }
		public string Database { get; set; }
		public string DatabaseSize { get; set; }
		public string SymbolsCount { get; set; }
		public string WordsCount { get; set; }
		public string LastUpdate { get; set; }
		public Visibility ProgressBarVisibility { get; set; }
		public string SymbolsInserting { get; set; }
		public int PercentOfSymbols { get; set; }
		public string WordsInserting { get; set; }
		public int PercentOfWords { get; set; }
		public bool CanUpdate { get; set; }
		public bool IsIndeterminate { get; set; }
		public string UpdateButtonContent { get; set; }
		public int FontSize { get; set; }

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
