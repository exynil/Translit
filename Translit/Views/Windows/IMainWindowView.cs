using Translit.Entity;

namespace Translit.Views.Windows
{
	interface IMainWindowView
	{
		void ShowNotification(string key);
		void BlockUnlockButtons();
		void UpdatePopupBox();
		void ClearAuthorizationForm();
		void ReloadFrame();
		void SetUserName(User user);
		void ClearUserName();
	}
}
