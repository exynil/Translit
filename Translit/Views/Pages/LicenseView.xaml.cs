using MaterialDesignThemes.Wpf;
using Translit.Presenters.Pages;
using System.Windows;
using System.Windows.Threading;
using Translit.Views.Windows;

namespace Translit.Views.Pages
{
	public partial class LicenseView : ILicenseView
	{
		private ILicensePresenter Presenter { get; }
		public Snackbar SnackbarNotification { get; set; }

		public LicenseView()
		{
			InitializeComponent();
			Presenter = new LicensePresenter(this);
		}

		private void LicenseView_OnLoaded(object sender, RoutedEventArgs e)
		{
			Presenter.LoadLicense();

			// Подключаем внешний уведомитель
			SnackbarNotification = ((MainWindowView)Window.GetWindow(this))?.SnackbarNotification;

			Loaded -= LicenseView_OnLoaded;
		}

		public void SetLicense(string text)
		{
			RichTextBoxLicense.AppendText(text);
		}

		public void ShowNotification(string message)
		{
			SnackbarNotification.Dispatcher.Invoke(() => { SnackbarNotification.MessageQueue.Enqueue(message); }, DispatcherPriority.Background);
		}
	}
}
