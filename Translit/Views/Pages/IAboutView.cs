namespace Translit.Views.Pages
{
	interface IAboutView
	{
		void SetTextBlockMargin(string text);
		void UpdateTextInTextBlock(string text);
		void SetLicense(string text);
	}
}
