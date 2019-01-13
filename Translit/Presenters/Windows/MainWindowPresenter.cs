using System.Threading.Tasks;
using Translit.Models.Windows;
using Translit.Properties;
using Translit.Views.Windows;

namespace Translit.Presenters.Windows
{
	public class MainWindowPresenter : IMainWindowPresenter
	{
		private IMainWindowModel Model { get; }

		private IMainWindowView View { get; }

		public MainWindowPresenter(MainWindowView view)
		{
			Model = new MainWindowModel();
			View = view;
		}
		public async void OnButtonSignInClicked(string login, string password)
		{
			// Выводим уведомление об авторизации
			View.ShowNotification("SnackBarAuthorization");

			// Делаем кнопку не активной
			View.BlockUnlockButtons();

			await Task.Factory.StartNew(() =>
			{
				if (Model.SignIn(login, password))
				{
					if (Settings.Default.IsUserAuthorized)
					{
						View.ShowNotification("SnackBarWelcome");
						View.ReloadFrame();
						View.ClearAuthorizationForm();
						View.SetUserName(Model.GetUser());
						View.UpdatePopupBox();
					}
					else
					{
						View.ShowNotification("SnackBarWrongLoginOrPassword");
					}
				}
				else
				{
					View.ShowNotification("SnackBarBadInternetConnection");
				}
			});

			// Делаем кнопку активной
			View.BlockUnlockButtons();
		}

		public void LogOut()
		{
			View.BlockUnlockButtons();
			Model.DeleteToken();
			Model.DeleteUserFromSettings();
			View.ReloadFrame();
			View.ClearUserName();
			View.UpdatePopupBox();
			View.BlockUnlockButtons();
		}

		public void OnWindowLoaded()
		{
			Model.MacAddressComparison();
			if (Settings.Default.IsUserAuthorized)
			{
				View.SetUserName(Model.GetUser());
			}
			View.UpdatePopupBox();
		}
	}
}
