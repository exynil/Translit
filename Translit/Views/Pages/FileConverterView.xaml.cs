using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Translit.Presenters.Pages;
using Application = System.Windows.Application;

namespace Translit.Views.Pages
{
	public partial class FileConverterView : IFileConverterView
	{
		private IFileConverterPresenter Presenter { get;}

		public FileConverterView()
		{
			InitializeComponent();
			Presenter = new FileConverterPresenter(this);
		}

		// Нажатие кнопки выбора файла Word
		private void ButtonSelectFile_Click(object sender, RoutedEventArgs e)
		{
			Presenter.OnButtonSelectFileClicked();
		}

		// Нажатие кнопки выбора папки с документами
		private void ButtonSelectFolder_Click(object sender, RoutedEventArgs e)
		{
			Presenter.OnButtonSelectFolderClicked();
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
			ProgressBarDocuments.Value = 0;
			ProgressBarDocuments.Maximum = numberOfDocuments;
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

		public void ShowNotification(string resource)
		{
			Task.Factory.StartNew(() => { })
				.ContinueWith(t => { SnackbarMain.MessageQueue.Enqueue(GetRes(resource)); },
					TaskScheduler.FromCurrentSynchronizationContext());
		}
	}
}