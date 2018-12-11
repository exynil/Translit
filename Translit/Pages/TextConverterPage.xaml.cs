using LiteDB;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Translit.Entity;

namespace Translit.Pages
{
	public partial class TextConverterPage
	{
		public TextConverterPage()
		{
			InitializeComponent();
		}

		public static string GetRtfStringFromRichTextBox(RichTextBox richTextBox)
		{
			TextRange textRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
			MemoryStream ms = new MemoryStream();
			textRange.Save(ms, DataFormats.Rtf);

			return Encoding.Default.GetString(ms.ToArray());
		}

		private void ButtonTranslit_OnClick(object sender, RoutedEventArgs e)
		{
			MemoryStream memoreStream = new MemoryStream(Encoding.ASCII.GetBytes(GetRtfStringFromRichTextBox(RichTextBoxCyryllic)));
			TextRange textRange = new TextRange(RichTextBoxLatin.Document.ContentStart, RichTextBoxLatin.Document.ContentEnd);
			textRange.Load(memoreStream, DataFormats.Rtf);

			TextRange latinTextRange = new TextRange(RichTextBoxLatin.Document.ContentStart, RichTextBoxLatin.Document.ContentEnd);
			TextPointer current = latinTextRange.Start.GetInsertionPosition(LogicalDirection.Forward);

			using (LiteDatabase db = new LiteDatabase(ConfigurationManager.ConnectionStrings["LiteDatabaseConnection"].ConnectionString))
			{
				var words = db.GetCollection<Word>("Words").FindAll().ToList();
				var symbols = db.GetCollection<Symbol>("Symbols").FindAll().ToList();

				while (current != null)
				{
					string textInRun = current.GetTextInRun(LogicalDirection.Forward);

					if (!string.IsNullOrWhiteSpace(textInRun))
					{
						current.DeleteTextInRun(textInRun.Length);
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
						current.InsertTextInRun(textInRun);
					}
					current = current.GetNextContextPosition(LogicalDirection.Forward);
				}
			}
		}

		// Очиста документа
		private void ButtonClear_OnClick(object sender, RoutedEventArgs e)
		{
			RichTextBoxCyryllic.Document.Blocks.Clear();
			RichTextBoxLatin.Document.Blocks.Clear();
		}

		// Копирование транслитезированного текста
		private void ButtonCopy_OnClick(object sender, RoutedEventArgs e)
		{
			TextRange range = new TextRange(RichTextBoxLatin.Document.ContentStart, RichTextBoxLatin.Document.ContentEnd);

			using (Stream stream = new MemoryStream())
			{
				range.Save(stream, DataFormats.Rtf);

				Clipboard.SetData(DataFormats.Rtf, Encoding.UTF8.GetString(((MemoryStream) stream).ToArray()));
			}
		}

		private void ToggleButtonRealTimeTransliteration_OnClick(object sender, RoutedEventArgs e)
		{
			if (ToggleButtonRealTimeTransliteration.IsChecked == true)
			{
				RichTextBoxCyryllic.KeyUp += ButtonTranslit_OnClick;
			}
			else
			{
				RichTextBoxCyryllic.KeyUp -= ButtonTranslit_OnClick;
			}
		}

		private void TextConverterPage_OnLoaded(object sender, RoutedEventArgs e)
		{
			if (ToggleButtonRealTimeTransliteration.IsChecked == true)
			{
				RichTextBoxCyryllic.KeyUp += ButtonTranslit_OnClick;
			}
			else
			{
				RichTextBoxCyryllic.KeyUp -= ButtonTranslit_OnClick;
			}
		}
	}
}
