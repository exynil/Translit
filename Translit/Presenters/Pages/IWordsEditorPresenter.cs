namespace Translit.Presenters.Pages
{
	interface IWordsEditorPresenter
	{
		void OnPageLoaded();
		void AddNewWord(string cyryllic, string latin);
		void EditWord(int id, string cyryllic, string latin);
		void DeleteWord(int id);
		void UpdateWords();
	}
}
