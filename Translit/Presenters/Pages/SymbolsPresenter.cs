using Translit.Models.Pages;
using Translit.Views.Pages;

namespace Translit.Presenters.Pages
{
	public class SymbolsPresenter : ISymbolsPresenter
	{
		private ISymbolsModel Model { get; }
		private ISymbolsView View { get; }

		public SymbolsPresenter(SymbolsView view)
		{
			Model = new SymbolsModel();
			View = view;
		}

		public void OnPageLoaded()
		{
			UpdateSymbols();
		}

		public void UpdateSymbols()
		{
			View.UpdateSymbols(Model.GetSymbolsFromDatabase());
		}
	}
}
