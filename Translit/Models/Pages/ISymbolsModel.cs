using System.Collections.Generic;
using Translit.Entity;

namespace Translit.Models.Pages
{
	interface ISymbolsModel
	{
		IEnumerable<Symbol> GetSymbolsFromDatabase();
	}
}
