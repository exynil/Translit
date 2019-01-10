namespace Translit.Views.Pages
{
	interface IFileConverterView
	{
		void BlockUnlockButtons();
		void ToggleProgressBarVisibility();
		void SetProgressBarStartValues(int numberOfDocuments);
		void ShowNotification(string message);
		void UpdateProgressValues(int numberOfDocumentsTranslated, int numberOfDocuments, int percentOfExceptions, int percentOfSymbols);
	}
}
