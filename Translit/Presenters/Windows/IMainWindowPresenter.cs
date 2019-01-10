namespace Translit.Presenters.Windows
{
	interface IMainWindowPresenter
	{
		void OnButtonSignInClicked();
		void OnWindowLoaded();
		void OnLanguageChanged();
		void OnListViewMenuSelectionChanged(int number);
		void LogOut();
		void OnTextBlockLoginChanged(string login);
		void OnPasswordBoxPasswordChanged(string password);
	}
}
