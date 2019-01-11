﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Translit.Entity;

namespace Translit.Models.Pages
{
	interface IWordsEditorModel
	{
		string ReasonPhrase { get; set; }

		bool CheckWordsLength(string cyryllic, string latin);
		Task AddWord(string cyryllic, string latin);
		Task EditWord(int id, string cyryllic, string latin);
		void DeleteWord(int id);
		IEnumerable<Word> GetWordsFromDatabase();
	}
}