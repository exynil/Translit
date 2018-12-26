using LiteDB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using Translit.Entity;

namespace Translit.Models.Pages
{
	public class DatabaseModel : INotifyPropertyChanged
	{
		private int _percentOfExceptions;
		private int _percentOfSymbols;
		public List<Symbol> Symbols { get; set; }
		public List<Word> Words { get; set; }
		public string ConnectionString { get; set; }

		public DatabaseModel()
		{
			ConnectionString = ConfigurationManager.ConnectionStrings["LiteDatabaseConnection"].ConnectionString;
		}

		public int PercentOfExceptions
		{
			get => _percentOfExceptions;
			set
			{
				if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
				_percentOfExceptions = value;
				OnPropertyChanged();
			}
		}

		public int PercentOfSymbols
		{
			get => _percentOfSymbols;
			set
			{
				if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
				_percentOfSymbols = value;
				OnPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChanged([CallerMemberName] string prop = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
		}

		public bool DownloadDatabaseFromServer()
		{
			string[] links = { @"http://translit.osmium.kz/api/symbol", @"http://translit.osmium.kz/api/word" };
			var request = new HttpWebRequest[2];
			request[0] = (HttpWebRequest)WebRequest.Create(links[0]);
			request[1] = (HttpWebRequest)WebRequest.Create(links[1]);
			var response = new HttpWebResponse[2];

			// Пытаем получить ответ от сервера
			try
			{
				response[0] = (HttpWebResponse)request[0].GetResponse();
				response[1] = (HttpWebResponse)request[1].GetResponse();
			}
			catch (Exception)
			{
				return false;
			}

			// Загружаем первый ответ сервера в базу
			var responseSrteam = response[0].GetResponseStream();
			using (var stream =
					new StreamReader(responseSrteam ?? throw new InvalidOperationException(), Encoding.UTF8))
			{
				// Десериализуем ответ
				Symbols = JsonConvert.DeserializeObject<List<Symbol>>(stream.ReadToEnd());
			}

			// Загружаем второй ответ сервера в базу
			responseSrteam = response[1].GetResponseStream();
			using (var stream =
					new StreamReader(responseSrteam ?? throw new InvalidOperationException(), Encoding.UTF8))
			{
				// Десериализуем ответ
				Words = JsonConvert.DeserializeObject<List<Word>>(stream.ReadToEnd());
			}

			return true;
		}

		public void DeleteOldDatabase()
		{
			// Удаляем локальную базу
			if (File.Exists(ConnectionString))
			{
				File.Delete(ConnectionString);
			}

			if (!Directory.Exists("Database"))
			{
				Directory.CreateDirectory("Database");
			}
		}

		public void InsertData()
		{
			using (var db = new LiteDatabase(ConnectionString))
			{
				// Создаем коллекцию
				var symbols = db.GetCollection<Symbol>("Symbols");

				// Перебираем полученный ответ от сервера и дабавляем каждое исключение в базу
				for (var i = 0; i < Symbols.Count; i++)
				{
					// Высчитываем процент выполнения
					PercentOfSymbols = i * 100 / (Symbols.Count - 1);

					// Добавляем символ в базу
					symbols.Upsert(Symbols[i]);
				}

				// Создаем коллекцию
				var words = db.GetCollection<Word>("Words");
				// Перебираем полученный ответ сервера и добавляем каждое исключение в базу
				for (var i = 0; i < Words.Count; i++)
				{
					// Высчитываем процент выполенения
					PercentOfExceptions = i * 100 / (Words.Count - 1);

					// Добавляем слово в базу
					words.Upsert(Words[i]);
				}
			}
		}

		public DatabaseInfo GetInfoAboutDatabase()
		{
			if (!File.Exists(ConnectionString)) return null;

			// Получаем информацию о файле
			var fileInfo = new FileInfo(ConnectionString);

			using (var db = new LiteDatabase(ConnectionString))
			{
				var databaseInfo = new DatabaseInfo
				{
					Name = fileInfo.Name,
					Length = fileInfo.Length,
					NumberOfSymbols = db.GetCollection<Symbol>("Symbols").Count(),
					NumberOfExceptions = db.GetCollection<Word>("Words").Count(),
					LastUpdate = fileInfo.LastWriteTime
				};
				return databaseInfo;
			}
		}
	}
}
