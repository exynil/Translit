namespace Translit.Presenters.Windows
{
	interface IMainWindowPresenter
	{
		void OnButtonSignInClicked(string login, string password);
		void OnWindowLoaded();
		void LogOut();
	}
}
