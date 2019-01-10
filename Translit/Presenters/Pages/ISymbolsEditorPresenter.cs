namespace Translit.Presenters.Pages
{
	interface ISymbolsEditorPresenter
	{
		void OnPageLoaded();
		void AddNewSymbol(string cyryllic, string latin);
		void EditSymbol(int id, string cyryllic, string latin);
		void DeleteSymbol(int id);
		void UpdateSymbols();
	}
}
