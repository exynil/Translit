using System.Configuration;
using System.IO;
using System.Linq;
using LiteDB;
using Translit.Models.Other;

namespace Translit.Models.Pages
{
    public class TextConverterModel : ITextConverterModel
    {
        public TextConverterModel()
        {
            ConnectionString = ConfigurationManager.ConnectionStrings["LiteDbConnection"].ConnectionString;
        }

        public string ConnectionString { get; set; }

        public string Transliterate(string text)
        {
            if (!File.Exists(ConnectionString)) return null;

            using (var db = new LiteDatabase(ConnectionString))
            {
                var words = db.GetCollection<Word>("Words").FindAll().ToArray();

                foreach (var w in words)
                    // Трансформируем слово в три различных состояния [ЗАГЛАВНЫЕ, прописные и Первыя заглавная] и заменяем
                    for (var j = 0; j < 3; j++)
                    {
                        var cyryllic = w.Cyryllic;
                        var latin = w.Latin;

                        switch (j)
                        {
                            case 0:
                                cyryllic = cyryllic.ToUpper();
                                latin = latin.ToUpper();
                                break;
                            case 1:
                                cyryllic = cyryllic.ToLower();
                                latin = latin.ToLower();
                                break;
                            case 2:
                                cyryllic = cyryllic.First().ToString().ToUpper() + cyryllic.Substring(1);
                                latin = latin.First().ToString().ToUpper() + latin.Substring(1);
                                break;
                        }

                        text = text.Replace(cyryllic, latin);
                    }

                var symbols = db.GetCollection<Symbol>("Symbols").FindAll().ToArray();

                return symbols.Aggregate(text, (current, s) => current.Replace(s.Cyryllic, s.Latin));
            }
        }
    }
}