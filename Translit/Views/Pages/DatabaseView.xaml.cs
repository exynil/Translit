using System.Configuration;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using MaterialDesignThemes.Wpf;
using Translit.Entity;
using Translit.Presenters.Pages;
using Translit.Views.Windows;

namespace Translit.Views.Pages
{
	public partial class DatabaseView : IDatabaseView
	{
		private IDatabasePresenter Presenter { get; }
		public Snackbar SnackbarNotification { get; set; }
		public string ConnectionString { get; set; }

		public DatabaseView()
		{
			InitializeComponent();
			Presenter = new DatabasePresenter(this);
			ConnectionString = ConfigurationManager.ConnectionStrings["LiteDatabaseConnection"].ConnectionString;
		}

		private void Page_OnLoaded(object sender, RoutedEventArgs e)
		{
			Presenter.OnPageLoaded();
			// Подключаем внешний уведомитель
			SnackbarNotification = ((MainWindowView)Window.GetWindow(this))?.SnackbarNotification;
			Loaded -= Page_OnLoaded;
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

		public void UpdateProgressValues(int percentOfSymbols, int percentOfWords)
		{
			ProgressBarSymbolInserting.Dispatcher.Invoke(() => ProgressBarSymbolInserting.Value = percentOfSymbols,
					DispatcherPriority.Background);
			ProgressBarWordInserting.Dispatcher.Invoke(() => ProgressBarWordInserting.Value = percentOfWords,
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
						TextBlockWordInserting.Text = GetRes("TextBlockLoadingWords") + ": " + percentOfWords + "%";
					},
					DispatcherPriority.Background);
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

		public void ShowNotification(string key)
		{
			SnackbarNotification.Dispatcher.Invoke(() =>
			{
				SnackbarNotification.MessageQueue.Enqueue(GetRes(key));
			}, DispatcherPriority.Background);
		}

		public void SetInfoAboutDatabase(DatabaseInfo databaseInfo)
		{
			if (databaseInfo != null)
			{
				TextBlockDatabase.Dispatcher.Invoke(() =>
				{
					TextBlockDatabase.SetResourceReference(TextBlock.TextProperty, "TextBlockDatabase");
				},
					DispatcherPriority.Background);

				TextBlockDatabaseSize.Dispatcher.Invoke(() =>
				{
					TextBlockDatabaseSize.SetResourceReference(TextBlock.TextProperty, "TextBlockDatabaseSize");
				}, DispatcherPriority.Background);

				TextBlockDatabaseSizeResult.Dispatcher.Invoke(() =>
				{
					// Выводим информацию о размере файла
					if (databaseInfo.Length < 1048576)
					{
						TextBlockDatabaseSizeResult.Text = databaseInfo.Length / 1024 + " KB";
					}
					else
					{
						TextBlockDatabaseSizeResult.Text = databaseInfo.Length / 1024 / 1024 + " MB";
					}
				}, DispatcherPriority.Background);

				TextBlockSymbolsCount.Dispatcher.Invoke(
					() =>
					{
						TextBlockSymbolsCount.SetResourceReference(TextBlock.TextProperty, "TextBlockAmountOfCharacters");
					}, DispatcherPriority.Background);

				TextBlockSymbolsCountResult.Dispatcher.Invoke(
					() =>
					{
						TextBlockSymbolsCountResult.Text = databaseInfo.NumberOfSymbols.ToString();
					}, DispatcherPriority.Background);

				TextBlockWordsCount.Dispatcher.Invoke(
					() =>
					{
						TextBlockWordsCount.SetResourceReference(TextBlock.TextProperty, "TextBlockAmountOfWords");
					}, DispatcherPriority.Background);

				TextBlockWordsCountResult.Dispatcher.Invoke(
					() => { TextBlockWordsCountResult.Text = databaseInfo.NumberOfWords.ToString(); }, DispatcherPriority.Background);

				TextBlockLastLastUpdate.Dispatcher.Invoke(
					() =>
					{
						TextBlockLastLastUpdate.SetResourceReference(TextBlock.TextProperty, "TextBlockLastUpdate");
					},
					DispatcherPriority.Background);

				TextBlockLastLastUpdateResult.Dispatcher.Invoke(
					() => { TextBlockLastLastUpdateResult.Text = databaseInfo.LastUpdate.ToString(CultureInfo.CurrentCulture); },
					DispatcherPriority.Background);
			}
			else
			{
				TextBlockDatabase.Dispatcher.Invoke(() =>
				{
					TextBlockDatabase.SetResourceReference(TextBlock.TextProperty, "TextBlockDatabaseNotFound");
				},
					DispatcherPriority.Background);
			}
		}
	}
}