using System.Collections.Generic;
using Translit.Entity;

namespace Translit.Views.Pages
{
	interface ISymbolsView
	{
		void UpdateSymbols(IEnumerable<Symbol> symbols);
	}
}
