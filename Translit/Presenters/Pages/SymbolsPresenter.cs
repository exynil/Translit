using Translit.Models.Pages;
using Translit.Views.Pages;

namespace Translit.Presenters.Pages
{
	public class SymbolsPresenter
	{
		public SymbolsModel Model { get; }
		public SymbolsView View { get; }

		public SymbolsPresenter(SymbolsModel model, SymbolsView view)
		{
			Model = model;
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
