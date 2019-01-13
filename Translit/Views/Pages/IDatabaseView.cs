using Translit.Entity;

namespace Translit.Views.Pages
{
	interface IDatabaseView
	{
		void ToggleUpdateButtonState(bool isActive);
		void ShowNotification(string key);
		void ToggleProgressBarVisibility();
		void SetInfoAboutDatabase(DatabaseInfo databaseInfo);
		void UpdateProgressValues(int percentOfSymbols, int percentOfExceptions);
	}
}