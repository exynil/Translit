using Translit.Models.Pages;
using Translit.Views.Pages;

namespace Translit.Presenters.Pages
{
	public class WordsPresenter
	{
		public WordsModel Model { get; }
		public WordsView View { get; }

		public WordsPresenter(WordsModel model, WordsView view)
		{
			Model = model;
			View = view;
		}

		public void OnPageLoaded()
		{
			UpdateWords();
		}

		public void UpdateWords()
		{
			View.UpdateWords(Model.GetWordsFromDatabase());
		}
	}
}
