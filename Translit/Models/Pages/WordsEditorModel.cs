﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using LiteDB;
using Newtonsoft.Json;
using Translit.Models.Other;
using Translit.Properties;

namespace Translit.Models.Pages
{
    public class WordsEditorModel : IWordsEditorModel
    {
        public string ConnectionString;

        public WordsEditorModel()
        {
            ConnectionString = ConfigurationManager.ConnectionStrings["LiteDbConnection"].ConnectionString;
            if (User.Token != null)
                JsonConvert.DeserializeObject<User>(Rc4.Calc(Settings.Default.FingerPrint, Settings.Default.User));
        }

        public string ReasonPhrase { get; set; }

        // Получение коллекции символов из базы
        public ObservableCollection<Word> GetWordsFromDatabase()
        {
            if (!File.Exists(ConnectionString)) return null;

            using (var db = new LiteDatabase(ConnectionString))
            {
                var temp = db.GetCollection<Word>("Words").FindAll().ToList();
                return new ObservableCollection<Word>(temp);
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
            const string link = "http://translit.osmium.kz/api/word?";
            var client = new HttpClient();

            // Создаем параметры
            var content =
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    {"token", User.Token},
                    {"cyrl", cyryllic},
                    {"latn", latin}
                });

            //Выполняем запрос
            var response = await client.PostAsync(link, content);

            // Полученный ответ десериализуем в объект Word
            var addedWord = JsonConvert.DeserializeObject<Word>(await response.Content.ReadAsStringAsync());

            // Выполняем следующие действия взамисимости от ответа сервера
            if (response.StatusCode == HttpStatusCode.Created)
                using (var db = new LiteDatabase(ConnectionString))
                {
                    var words = db.GetCollection<Word>("Words");
                    words.Insert(addedWord);
                }

            ReasonPhrase = response.ReasonPhrase;
        }

        public async Task EditWord(int id, string cyryllic, string latin)
        {
            const string link = "http://translit.osmium.kz/api/word?";
            var client = new HttpClient();

            // Создаем параметры
            var values = new Dictionary<string, string> {{"token", User.Token}, {"id", id.ToString()}};
            if (cyryllic != null) values.Add("cyrl", cyryllic);
            if (latin != null) values.Add("latn", latin);
            var content = new FormUrlEncodedContent(values);

            // Выполняем запрос
            var response = await client.PutAsync(link, content);
            if (response.StatusCode == HttpStatusCode.OK)
                using (var db = new LiteDatabase(ConnectionString))
                {
                    var words = db.GetCollection<Word>("Words");
                    var word = words.FindById(id);
                    if (cyryllic != null) word.Cyryllic = cyryllic;
                    if (latin != null) word.Latin = latin;
                    words.Update(word);
                }

            ReasonPhrase = response.ReasonPhrase;
        }

        public void DeleteWord(int id)
        {
            // Строим адрес
            var link = $"http://translit.osmium.kz/api/word?token={User.Token}&id={id}";
            var client = new HttpClient();

            // Выполняем запрос
            var response = client.DeleteAsync(link).Result;

            // Выполняем следующие действия взамисимости от ответа сервера
            if (response.StatusCode == HttpStatusCode.OK)
                using (var db = new LiteDatabase(ConnectionString))
                {
                    // Получаем коллекцию Words
                    var words = db.GetCollection<Word>("Words");
                    // Удаляем выбранный Id
                    words.Delete(id);
                }

            ReasonPhrase = response.ReasonPhrase;
        }
    }
}