using System.Collections.Generic;
using Translit.Entity;

namespace Translit.Views.Pages
{
	interface IWordsEditorView
	{
		void ShowNotification(string message);
		void UpdateWords(IEnumerable<Word> words);
	}
}
