namespace Translit.Views.Pages
{
	interface IFileConverterView
	{
		void BlockUnlockButtons();
		void ToggleProgressBarVisibility();
		void SetProgressBarStartValues(int amountOfDocuments);
		void ShowNotification(string key);
		void UpdateProgressValues(int amountOfTranslatedDocuments, int amountOfDocuments, int percentOfWords, int percentOfSymbols);
	}
}
