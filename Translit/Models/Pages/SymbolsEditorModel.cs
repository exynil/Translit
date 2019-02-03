using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using LiteDB;
using Newtonsoft.Json;
using Translit.Entity;
using Translit.Properties;

namespace Translit.Models.Pages
{
	public class SymbolsEditorModel : ISymbolsEditorModel
	{
		public string ReasonPhrase { get; set; }
		public string ConnectionString { get; }

		public SymbolsEditorModel()
		{
			ConnectionString = ConfigurationManager.ConnectionStrings["LiteDatabaseConnection"].ConnectionString;
		}

		// Получение коллекции символов из базы
		public ObservableCollection<Symbol> GetSymbolsFromDatabase()
		{
			if (!File.Exists(ConnectionString)) return null;

			using (var db = new LiteDatabase(ConnectionString))
			{
				var temp = db.GetCollection<Symbol>("Symbols").FindAll().ToList();
				return new ObservableCollection<Symbol>(temp);
			}
		}

		// Проверка длины допустимого значения
		public bool CheckSymbolsLength(string cyryllic, string latin)
		{
			return !(cyryllic?.Length > 5) && !(latin?.Length > 5);
		}

		// Добавление символа в глобальную базу
		public async Task AddSymbol(string cyryllic, string latin)
		{
			var user = JsonConvert.DeserializeObject<User>(Rc4.Calc(Settings.Default.MacAddress, Settings.Default.User));

			const string link = "http://translit.osmium.kz/api/symbol?";

			var client = new HttpClient();

			// Создаем параметры
			var content = new FormUrlEncodedContent(new Dictionary<string, string>
			{
				{"token", user.Token},
				{"cyrl", cyryllic},
				{"latn", latin}
			});

			//Выполняем запрос
			var response = await client.PostAsync(link, content);

			// Полученный ответ десериализуем в объект Word
			var addedSymbol = JsonConvert.DeserializeObject<Symbol>(await response.Content.ReadAsStringAsync());

			// Выполняем следующие действия взамисимости от ответа сервера
			if (response.StatusCode == HttpStatusCode.Created)
			{
				// Добавляем слово в локальную базу
				using (var db = new LiteDatabase(ConnectionString))
				{
					var symbols = db.GetCollection<Symbol>("Symbols");
					symbols.Insert(addedSymbol);
				}
			}

			ReasonPhrase = response.ReasonPhrase;
		}

		public async Task EditSymbol(int id, string cyryllic, string latin)
		{
			var user = JsonConvert.DeserializeObject<User>(Rc4.Calc(Settings.Default.MacAddress, Settings.Default.User));

			const string link = "http://translit.osmium.kz/api/symbol?";

			var client = new HttpClient();

			// Создаем параметры
			var values = new Dictionary<string, string>
			{
				{"token", user.Token},
				{"id", id.ToString() }
			};

			if (cyryllic != null) values.Add("cyrl", cyryllic);
			if (latin != null) values.Add("latn", latin);

			var content = new FormUrlEncodedContent(values);

			// Выполняем запрос
			var response = await client.PutAsync(link, content);

			if (response.StatusCode == HttpStatusCode.OK)
			{
				// Изменяем слово в локальной базе
				using (var db = new LiteDatabase(ConnectionString))
				{
					var symbols = db.GetCollection<Symbol>("Symbols");
					var symbol = symbols.FindById(id);
					if (cyryllic != null) symbol.Cyryllic = cyryllic;
					if (latin != null) symbol.Latin = latin;
					symbols.Update(symbol);
				}
			}

			ReasonPhrase = response.ReasonPhrase;
		}

		public void DeleteSymbol(int id)
		{
			var user = JsonConvert.DeserializeObject<User>(Rc4.Calc(Settings.Default.MacAddress, Settings.Default.User));

			// Строим адрес
			var link = $"http://translit.osmium.kz/api/symbol?token={user.Token}&id={id}";

			var client = new HttpClient();

			// Выполняем запрос
			var response = client.DeleteAsync(link).Result;

			// Выполняем следующие действия взамисимости от ответа сервера
			if (response.StatusCode == HttpStatusCode.OK)
			{
				using (var db = new LiteDatabase(ConnectionString))
				{
					// Получаем коллекцию Words
					var symbols = db.GetCollection<Symbol>("Symbols");
					// Удаляем выбранный Id
					symbols.Delete(id);
				}
			}

			ReasonPhrase = response.ReasonPhrase;
		}
	}
}