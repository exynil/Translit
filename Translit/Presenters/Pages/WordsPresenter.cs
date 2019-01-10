using Translit.Models.Pages;
using Translit.Views.Pages;

namespace Translit.Presenters.Pages
{
	public class WordsPresenter : IWordsPresenter
	{
		private IWordsModel Model { get; }
		private IWordsView View { get; }

		public WordsPresenter(WordsView view)
		{
			Model = new WordsModel();
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
