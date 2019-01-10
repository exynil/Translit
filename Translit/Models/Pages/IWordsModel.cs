using System.Collections.Generic;
using Translit.Entity;

namespace Translit.Models.Pages
{
	interface IWordsModel
	{
		IEnumerable<Word> GetWordsFromDatabase();
	}
}
