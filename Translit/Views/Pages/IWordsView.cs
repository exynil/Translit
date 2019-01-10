using System.Collections.Generic;
using Translit.Entity;

namespace Translit.Views.Pages
{
	interface IWordsView
	{
		void UpdateWords(IEnumerable<Word> words);
	}
}
