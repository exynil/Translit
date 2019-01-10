using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Translit.Entity;

namespace Translit.Views.Windows
{
	interface IMainWindowView
	{
		void ShowNotification(string message);
		void BlockUnlockButtons();
		void UpdateRightMenu(User user);
		void ClearAuthorizationForm();
		void SetTitle(int number);
		void ChangePage(int number);
		void SelectLanguage();
		void RefreshFrame();
		void ChangeListViewItemColor();
	}
}
