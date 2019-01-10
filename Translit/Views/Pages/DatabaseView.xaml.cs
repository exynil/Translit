using System.Configuration;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using MaterialDesignThemes.Wpf;
using Translit.Entity;
using Translit.Presenters.Pages;

namespace Translit.Views.Pages
{
	public partial class DatabaseView : IDatabaseView
	{
		private IDatabasePresenter Presenter { get; set; }
		public string ConnectionString { get; set; }

		public DatabaseView()
		{
			InitializeComponent();
			Presenter = new DatabasePresenter(this);
			ConnectionString = ConfigurationManager.ConnectionStrings["LiteDatabaseConnection"].ConnectionString;
		}

		// Нажатие кнопки обновления базы
		private void ButtonDatabaseUpdate_OnClick(object sender, RoutedEventArgs e)
		{
			Presenter.DownloadDatabase();
		}

		public void ToggleUpdateButtonState(bool isActive)
		{
			if (isActive)
			{
				ButtonDatabaseUpdate.Dispatcher.Invoke(() =>
				{
					ButtonDatabaseUpdate.SetValue(ButtonProgressAssist.IsIndeterminateProperty, false);
					ButtonDatabaseUpdate.Content = GetRes("ButtonUpdate");
					ButtonDatabaseUpdate.Click += ButtonDatabaseUpdate_OnClick;
				}, DispatcherPriority.Background);
			}
			else
			{
				ButtonDatabaseUpdate.Dispatcher.Invoke(() =>
				{
					ButtonDatabaseUpdate.SetValue(ButtonProgressAssist.IsIndeterminateProperty, true);
					ButtonDatabaseUpdate.Content = GetRes("ButtonLoading");
					ButtonDatabaseUpdate.Click -= ButtonDatabaseUpdate_OnClick;
				}, DispatcherPriority.Background);
			}
		}

		public void UpdateProgressValues(int percentOfSymbols, int percentOfExceptions)
		{
			ProgressBarSymbolInserting.Dispatcher.Invoke(() => ProgressBarSymbolInserting.Value = percentOfSymbols,
					DispatcherPriority.Background);
			ProgressBarWordInserting.Dispatcher.Invoke(() => ProgressBarWordInserting.Value = percentOfExceptions,
					DispatcherPriority.Background);

			TextBlockSymbolInserting.Dispatcher.Invoke(
					() =>
					{
						TextBlockSymbolInserting.Text = GetRes("TextBlockCharacterLoading") + ": " + percentOfSymbols + "%";
					},
					DispatcherPriority.Background);

			TextBlockWordInserting.Dispatcher.Invoke(
					() =>
					{
						TextBlockWordInserting.Text = GetRes("TextBlockLoadingExceptions") + ": " + percentOfExceptions + "%";
					},
					DispatcherPriority.Background);
		}

		// Первоначальная загрузка страницы
		private void Database_OnLoaded(object sender, RoutedEventArgs e)
		{
			Presenter.OnPageLoaded();
		}

		public string GetRes(string key)
		{
			return Application.Current.Resources[key].ToString();
		}

		public void ToggleProgressBarVisibility()
		{
			var visibility = StackPanelProgress.Dispatcher.Invoke(() => StackPanelProgress.Visibility,
					DispatcherPriority.Background);

			visibility = visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;

			StackPanelProgress.Dispatcher.Invoke(() => StackPanelProgress.Visibility = visibility,
					DispatcherPriority.Background);
		}

		public void ShowNotification(string resource)
		{
			Task.Factory.StartNew(() => { })
					.ContinueWith(t =>
					{
						SnackbarMain.Dispatcher.Invoke(() => { SnackbarMain.MessageQueue.Enqueue(GetRes(resource)); }, DispatcherPriority.Background);
					});
		}

		public void SetInfoAboutDatabase(DatabaseInfo databaseInfo)
		{
			if (databaseInfo != null)
			{
				TextBlockDatabase.Dispatcher.Invoke(() => { TextBlockDatabase.Text = GetRes("TextBlockDatabase"); },
					DispatcherPriority.Background);

				TextBlockDatabaseSize.Dispatcher.Invoke(() =>
				{
					// Выводим информацию о размере файла
					if (databaseInfo.Length < 1048576)
					{
						var text = GetRes("TextBlockDatabaseSize") + ": " + databaseInfo.Length / 1024 + GetRes("FileKb");
						TextBlockDatabaseSize.Text = text;
					}
					else
					{
						var text = GetRes("TextBlockDatabaseSize") + ": " + databaseInfo.Length / 1024 / 1024 + GetRes("FileMb");
						TextBlockDatabaseSize.Text = text;
					}
				}, DispatcherPriority.Background);

				TextBlockSymbolsCount.Dispatcher.Invoke(
					() =>
					{
						TextBlockSymbolsCount.Text = GetRes("TextBlockAmountOfCharacters") + ": " + databaseInfo.NumberOfSymbols;
					}, DispatcherPriority.Background);
				TextBlockWordsCount.Dispatcher.Invoke(
					() =>
					{
						TextBlockWordsCount.Text = GetRes("TextBlockAmountOfWords") + ": " + databaseInfo.NumberOfExceptions;
					}, DispatcherPriority.Background);

				TextBlockWordsCount.Dispatcher.Invoke(
					() => { TextBlockLastLastUpdate.Text = GetRes("TextBlockLastUpdate") + ": " + databaseInfo.LastUpdate; },
					DispatcherPriority.Background);
			}
			else
			{
				TextBlockDatabase.Dispatcher.Invoke(() => { TextBlockDatabase.Text = GetRes("TextBlockDatabaseNotFound"); },
					DispatcherPriority.Background);
			}
		}
	}
}