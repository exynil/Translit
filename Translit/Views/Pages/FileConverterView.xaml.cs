using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using MaterialDesignThemes.Wpf;
using Translit.Presenters.Pages;
using Translit.Views.Windows;
using Application = System.Windows.Application;

namespace Translit.Views.Pages
{
	public partial class FileConverterView : IFileConverterView
	{
		private IFileConverterPresenter Presenter { get;}
		public Snackbar SnackbarNotification { get; set; }
		public FileConverterView()
		{
			InitializeComponent();
			Presenter = new FileConverterPresenter(this);
		}

		private void Page_OnLoaded(object sender, RoutedEventArgs e)
		{
			// Подключаем внешний уведомитель
			SnackbarNotification = ((MainWindowView)Window.GetWindow(this))?.SnackbarNotification;

			Loaded -= Page_OnLoaded;
		}

		// Нажатие кнопки выбора файла Word
		private void ButtonSelectFile_Click(object sender, RoutedEventArgs e)
		{
			// Создаем экземпляр диалогового окна выбора файла
			var dlg = new Microsoft.Win32.OpenFileDialog()
			{
				FileName = "Document",
				DefaultExt = ".*",
				Filter = "Text documents (.doc; .docx)|*.doc;*.docx" // Фильтрация файлов
			};

			// Открываем диалоговое окно
			var result = dlg.ShowDialog();

			if (!File.Exists(@"Database\localdb.db"))
			{
				ShowNotification("SnackBarDatabaseNotFound");
				return;
			}

			if (result != true) return;

			Presenter.TranslitFiles(new[] {dlg.FileName});
		}

		// Нажатие кнопки выбора папки с документами
		private void ButtonSelectFolder_Click(object sender, RoutedEventArgs e)
		{
			using (var fbd = new FolderBrowserDialog())
			{
				// Открываем диалоговое окно
				var result = fbd.ShowDialog();

				if (!File.Exists(@"Database\localdb.db"))
				{
					ShowNotification("SnackBarDatabaseNotFound");
					return;
				}

				if (result != DialogResult.OK || string.IsNullOrWhiteSpace(fbd.SelectedPath)) return;

				// Получаем пути всех нескрытых файлов с расширением .doc и .docx
				var files = new DirectoryInfo(fbd.SelectedPath).EnumerateFiles()
					.Where(f => (f.Attributes & FileAttributes.Hidden) == 0 && (f.Extension == ".doc" || f.Extension == ".docx"))
					.Select(f => f.FullName).ToArray();

				Presenter.TranslitFiles(files);
			}
		}

		public string GetRes(string key)
		{
			return Application.Current.Resources[key].ToString();
		}

		public void BlockUnlockButtons()
		{
			ButtonSelectFile.IsEnabled = !ButtonSelectFile.IsEnabled;
			ButtonSelectFolder.IsEnabled = !ButtonSelectFolder.IsEnabled;
		}

		public void ToggleProgressBarVisibility()
		{
			var visibility = StackPanelProgress.Dispatcher.Invoke(() => StackPanelProgress.Visibility,
				DispatcherPriority.Background);

			visibility = visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;

			StackPanelProgress.Dispatcher.Invoke(() => StackPanelProgress.Visibility = visibility,
				DispatcherPriority.Background);
		}

		public void SetProgressBarStartValues(int numberOfDocuments)
		{
			ProgressBarDocuments.Maximum = numberOfDocuments;
			ProgressBarDocuments.Value = 0;
			ProgressBarExceptions.Value = 0;
			ProgressBarSymbols.Value = 0;
		}

		public void UpdateProgressValues(int numberOfDocumentsTranslated, int numberOfDocuments, int percentOfExceptions, int percentOfSymbols)
		{
			// Обновляем процесс
			TextBlockDocumetns.Dispatcher.Invoke(
				() => { TextBlockDocumetns.Text = GetRes("TextBlockDocuments") + ": " + numberOfDocumentsTranslated + "/" + numberOfDocuments; },
				DispatcherPriority.Background);
			ProgressBarDocuments.Dispatcher.Invoke(() => { ProgressBarDocuments.Value = numberOfDocumentsTranslated; },
				DispatcherPriority.Background);

			TextBlockExceptions.Dispatcher.Invoke(
				() => { TextBlockExceptions.Text = GetRes("TextBlockTransliterationOfExceptionWords") + ": " + percentOfExceptions + "%"; },
				DispatcherPriority.Background);
			ProgressBarExceptions.Dispatcher.Invoke(() => { ProgressBarExceptions.Value = percentOfExceptions; },
				DispatcherPriority.Background);

			TextBlockSymbols.Dispatcher.Invoke(
				() => TextBlockSymbols.Text = GetRes("TextBlockCharacterTransliteration") + ": " + percentOfSymbols + "%",
				DispatcherPriority.Background);
			ProgressBarSymbols.Dispatcher.Invoke(() => { ProgressBarSymbols.Value = percentOfSymbols; },
				DispatcherPriority.Background);
		}

		public void ShowNotification(string key)
		{
			SnackbarNotification.Dispatcher.Invoke(() => { SnackbarNotification.MessageQueue.Enqueue(GetRes(key)); },
				DispatcherPriority.Background);
		}
	}
}