using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Translit.Models.Other;

namespace Translit.Models.Pages
{
    internal interface ISymbolsEditorModel
    {
        string ReasonPhrase { get; set; }
        bool CheckSymbolsLength(string cyryllic, string latin);
        Task AddSymbol(string cyryllic, string latin);
        Task EditSymbol(int id, string cyryllic, string latin);
        void DeleteSymbol(int id);
        ObservableCollection<Symbol> GetSymbolsFromDatabase();
    }
}