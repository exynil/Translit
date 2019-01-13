using System.Collections.Generic;
using Translit.Entity;

namespace Translit.Views.Pages
{
	interface ISymbolsEditorView
	{
		void ShowNotification(string key);
		void UpdateSymbols(IEnumerable<Symbol> symbols);
	}
}
