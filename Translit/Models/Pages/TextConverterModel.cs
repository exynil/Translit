using LiteDB;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using Translit.Entity;

namespace Translit.Models.Pages
{
	public class TextConverterModel : ITextConverterModel
	{
		public TextRange LatinTextRange { get; set; }

		public void TranslitTextRange(TextRange textRange)
		{
			LatinTextRange = textRange;

			TextPointer textPointer = LatinTextRange.Start.GetInsertionPosition(LogicalDirection.Forward);

			using (LiteDatabase db = new LiteDatabase(ConfigurationManager.ConnectionStrings["LiteDatabaseConnection"].ConnectionString))
			{
				var words = db.GetCollection<Word>("Words").FindAll().ToList();
				var symbols = db.GetCollection<Symbol>("Symbols").FindAll().ToList();

				while (textPointer != null)
				{
					string textInRun = textPointer.GetTextInRun(LogicalDirection.Forward);

					if (!string.IsNullOrWhiteSpace(textInRun))
					{
						textPointer.DeleteTextInRun(textInRun.Length);
						foreach (var w in words)
						{
							string cyryllic = w.Cyryllic;
							string latin = w.Latin;

							// Замена слова в верхнем регистре
							cyryllic = cyryllic.ToUpper();
							latin = latin.ToUpper();
							textInRun = textInRun.Replace(cyryllic, latin);

							// Замена слова в нижнем регистре
							cyryllic = cyryllic.ToLower();
							latin = latin.ToLower();
							textInRun = textInRun.Replace(cyryllic, latin);

							// Замена слова с первой заглавной буквой
							cyryllic = cyryllic.First().ToString().ToUpper() + cyryllic.Substring(1);
							latin = latin.First().ToString().ToUpper() + latin.Substring(1);
							textInRun = textInRun.Replace(cyryllic, latin);
						}

						foreach (var s in symbols)
						{
							textInRun = textInRun.Replace(s.Cyryllic, s.Latin);
						}
						textPointer.InsertTextInRun(textInRun);
					}
					textPointer = textPointer.GetNextContextPosition(LogicalDirection.Forward);
				}
			}
		}

		public void CopyToClipboard(TextRange textRange)
		{
			using (Stream stream = new MemoryStream())
			{
				textRange.Save(stream, DataFormats.Rtf);

				Clipboard.SetData(DataFormats.Rtf, Encoding.UTF8.GetString(((MemoryStream)stream).ToArray()));
			}
		}
	}
}
