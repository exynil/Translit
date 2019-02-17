using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using LiteDB;
using Newtonsoft.Json;
using Translit.Models.Other;

namespace Translit.Models.Pages
{
    public class DatabaseModel : IDatabaseModel, INotifyPropertyChanged
    {
        private int _percentOfSymbols;
        private int _percentOfWords;

        public DatabaseModel()
        {
            ConnectionString = ConfigurationManager.ConnectionStrings["LiteDatabaseConnection"].ConnectionString;
        }

        public int PercentOfWords
        {
            get => _percentOfWords;
            set
            {
                _percentOfWords = value;
                OnPropertyChanged();
            }
        }

        public int PercentOfSymbols
        {
            get => _percentOfSymbols;
            set
            {
                _percentOfSymbols = value;
                OnPropertyChanged();
            }
        }

        public List<Symbol> Symbols { get; set; }
        public List<Word> Words { get; set; }
        public string ConnectionString { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool DownloadOrUpdateDatabase()
        {
            string[] links = {@"http://translit.osmium.kz/api/symbol", @"http://translit.osmium.kz/api/word"};
            var request = new HttpWebRequest[2];
            request[0] = (HttpWebRequest) WebRequest.Create(links[0]);
            request[1] = (HttpWebRequest) WebRequest.Create(links[1]);
            var response = new HttpWebResponse[2];

            // Пытаем получить ответ от сервера
            try
            {
                response[0] = (HttpWebResponse) request[0].GetResponse();
                response[1] = (HttpWebResponse) request[1].GetResponse();
            }
            catch (Exception)
            {
                return false;
            }

            // Загружаем первый ответ сервера
            var responseSrteam = response[0].GetResponseStream();

            if (responseSrteam == null) return false;

            using (var stream = new StreamReader(responseSrteam, Encoding.UTF8))
            {
                // Десериализуем ответ
                Symbols = JsonConvert.DeserializeObject<List<Symbol>>(stream.ReadToEnd());
            }

            // Загружаем второй ответ сервера
            responseSrteam = response[1].GetResponseStream();

            if (responseSrteam == null) return false;

            using (var stream = new StreamReader(responseSrteam, Encoding.UTF8))
            {
                // Десериализуем ответ
                Words = JsonConvert.DeserializeObject<List<Word>>(stream.ReadToEnd());
            }

            return true;
        }

        public void InsertData()
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                if (db.CollectionExists("Symbols")) db.DropCollection("Symbols");

                if (db.CollectionExists("Words")) db.DropCollection("Words");

                BsonMapper.Global.EmptyStringToNull = false;

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
                    PercentOfWords = i * 100 / (Words.Count - 1);

                    // Добавляем слово в базу
                    words.Upsert(Words[i]);
                }
            }

            Symbols.Clear();
            Words.Clear();
        }

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public bool CollectionExists()
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                return db.CollectionExists("Symbols") && db.CollectionExists("Words");
            }
        }

        public long GetDatabaseSize()
        {
            return new FileInfo(ConnectionString).Length;
        }

        public int GetSymbolCount()
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                return db.GetCollection<Symbol>("Symbols").Count();
            }
        }

        public int GetWordCount()
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                return db.GetCollection<Word>("Words").Count();
            }
        }

        public DateTime GetLastUpdateTime()
        {
            return new FileInfo(ConnectionString).LastWriteTime;
        }
    }
}