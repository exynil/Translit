using LiteDB;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Translit.Entity;

namespace Translit.Models.Pages
{
	public class SymbolsModel : ISymbolsModel
	{
		public string ConnectionString { get; }

		public SymbolsModel()
		{
			ConnectionString = ConfigurationManager.ConnectionStrings["LiteDatabaseConnection"].ConnectionString;
		}

		// Получение коллекции символов из базы
		public IEnumerable<Symbol> GetSymbolsFromDatabase()
		{
			using (var db = new LiteDatabase(ConnectionString))
			{
				return db.GetCollection<Symbol>("Symbols").FindAll().ToList();
			}
		}
	}
}
