using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Translit.Models.Other;

namespace Translit.Models.Pages
{
    interface IWordsEditorModel
	{
		string ReasonPhrase { get; set; }

		bool CheckWordsLength(string cyryllic, string latin);
		Task AddWord(string cyryllic, string latin);
		Task EditWord(int id, string cyryllic, string latin);
		void DeleteWord(int id);
		ObservableCollection<Word> GetWordsFromDatabase();
	}
}
