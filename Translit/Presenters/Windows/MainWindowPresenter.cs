using Translit.Models.Windows;
using Translit.Views.Windows;

namespace Translit.Presenters.Windows
{
	public class MainWindowPresenter
	{
		public MainWindowModel Model { get; }

		public MainWindowView View { get; }

		public MainWindowPresenter(MainWindowModel model, MainWindowView view)
		{
			Model = model;
			View = view;
		}

		public void OnTextBlockLoginChanged(string login)
		{
			Model.Login = login;
		}

		public void OnPasswordBoxPasswordChanged(string password)
		{
			Model.Password = password;
		}

		public void OnButtonSignInClicked()
		{
			// Проверяем логин или пароль на заполненность
			if (Model.IsLoginOrPasswordEmpty()) return;

			// Выводим уведомление об авторизации
			View.ShowNotification("SnackBarAuthorization");

			// Делаем кнопку не активной
			View.BlockUnlockButtons();

			if (Model.SignIn() != null)
			{
				View.ShowNotification("SnackBarWelcome");
				View.UpdateRightMenu(Model.GetUser());
				// -----------------------------------
				// Тут требуется перезагрузка страницы
				// -----------------------------------
				View.ClearAuthorizationForm();
				View.UpdateRightMenu(Model.GetUser());
			}
			else
			{
				View.ShowNotification("SnackBarWrongLoginOrPassword");
			}

			// Делаем кнопку активной
			View.BlockUnlockButtons();
		}

		public void LogOut()
		{
			View.BlockUnlockButtons();
			Model.DeleteToken();
			Model.DeleteUserFromSettings();
			View.UpdateRightMenu(Model.GetUser());
			View.BlockUnlockButtons();
		}

		public void OnWindowLoaded()
		{
			Model.MacAddressComparison();
			View.UpdateRightMenu(Model.GetUser());
			View.SetTitle(0);
			View.ChangePage(0);
		}

		public void OnLanguageChanged()
		{
			View.SelectLanguage();
			View.RefreshFrame();
		}

		public void OnListViewMenuSelectionChanged(int number)
		{
			View.ChangeListViewItemColor();
			View.SetTitle(number);
			View.ChangePage(number);
		}
	}
}
