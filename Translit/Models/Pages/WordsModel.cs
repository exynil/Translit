using LiteDB;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Translit.Entity;

namespace Translit.Models.Pages
{
	public class WordsModel
	{
		public string ConnectionString { get; }

		public WordsModel()
		{
			ConnectionString = ConfigurationManager.ConnectionStrings["LiteDatabaseConnection"].ConnectionString;
		}

		// Получение коллекции символов из базы
		public IEnumerable<Word> GetWordsFromDatabase()
		{
			using (var db = new LiteDatabase(ConnectionString))
			{
				return db.GetCollection<Word>("Words").FindAll().ToList();
			}
		}
	}
}
