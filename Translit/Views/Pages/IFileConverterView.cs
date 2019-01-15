namespace Translit.Views.Pages
{
	interface IFileConverterView
	{
		void BlockUnlockButtons();
		void ToggleProgressBarVisibility();
		void SetProgressBarStartValues(int numberOfDocuments);
		void ShowNotification(string key);
		void UpdateProgressValues(int numberOfDocumentsTranslated, int numberOfDocuments, int percentOfWords, int percentOfSymbols);
	}
}
