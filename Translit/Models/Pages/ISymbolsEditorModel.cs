using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Translit.Entity;

namespace Translit.Models.Pages
{
	interface ISymbolsEditorModel
	{
		bool CheckSymbolsLength(string cyryllic, string latin);
		Task AddSymbol(string cyryllic, string latin);
		string ReasonPhrase { get; set; }
		Task EditSymbol(int id, string cyryllic, string latin);
		void DeleteSymbol(int id);
		ObservableCollection<Symbol> GetSymbolsFromDatabase();
	}
}
