using LiteDB;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Translit.Entity;
using Translit.Properties;

namespace Translit.Models.Pages
{
	public class WordsEditorModel : IWordsEditorModel
	{
		public string ReasonPhrase { get; set; }
		public string ConnectionString;

		public WordsEditorModel()
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

		// Проверка длины допустимого значения
		public bool CheckWordsLength(string cyryllic, string latin)
		{
			return !(cyryllic?.Length > 30) && !(latin?.Length > 40);
		}

		// Добавление символа в глобальную базу
		public async Task AddWord(string cyryllic, string latin)
		{
			var user = JsonConvert.DeserializeObject<User>(Rc4.Calc(Settings.Default.MacAddress, Settings.Default.User));
			const string link = "http://translit.osmium.kz/api/word?";
			var client = new HttpClient();

			// Создаем параметры
			var content =
				new FormUrlEncodedContent(new Dictionary<string, string>
				{
					{"token", user.Token},
					{"cyrl", cyryllic},
					{"latn", latin}
				});

			//Выполняем запрос
			var response = await client.PostAsync(link, content);

			// Полученный ответ десериализуем в объект Word
			var addedWord = JsonConvert.DeserializeObject<Word>(await response.Content.ReadAsStringAsync());

			// Выполняем следующие действия взамисимости от ответа сервера
			if (response.StatusCode == HttpStatusCode.Created)
			{
				// Добавляем слово в локальную базу
				using (var db = new LiteDatabase(ConnectionString))
				{
					var words = db.GetCollection<Word>("Words");
					words.Insert(addedWord);
				}
			}

			ReasonPhrase = response.ReasonPhrase;
		}

		public async Task EditWord(int id, string cyryllic, string latin)
		{
			var user = JsonConvert.DeserializeObject<User>(Rc4.Calc(Settings.Default.MacAddress, Settings.Default.User));
			const string link = "http://translit.osmium.kz/api/word?";
			var client = new HttpClient();

			// Создаем параметры
			var values = new Dictionary<string, string> {{"token", user.Token}, {"id", id.ToString()}};
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
					var words = db.GetCollection<Word>("Words");
					var word = words.FindById(id);
					if (cyryllic != null) word.Cyryllic = cyryllic;
					if (latin != null) word.Latin = latin;
					words.Update(word);
				}
			}

			ReasonPhrase = response.ReasonPhrase;
		}

		public void DeleteWord(int id)
		{
			var user = JsonConvert.DeserializeObject<User>(Rc4.Calc(Settings.Default.MacAddress, Settings.Default.User));

			// Строим адрес
			var link = $"http://translit.osmium.kz/api/word?token={user.Token}&id={id}";
			var client = new HttpClient();

			// Выполняем запрос
			var response = client.DeleteAsync(link).Result;

			// Выполняем следующие действия взамисимости от ответа сервера
			if (response.StatusCode == HttpStatusCode.OK)
			{
				using (var db = new LiteDatabase(ConnectionString))
				{
					// Получаем коллекцию Words
					var words = db.GetCollection<Word>("Words");
					// Удаляем выбранный Id
					words.Delete(id);
				}
			}

			ReasonPhrase = response.ReasonPhrase;
		}
	}
}