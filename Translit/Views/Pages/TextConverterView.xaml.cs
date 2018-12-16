using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using Translit.Models.Pages;
using Translit.Presenters.Pages;

namespace Translit.Views.Pages
{
	public partial class TextConverterView
	{
		private TextConverterPresenter Presenter { get; }
		public TextConverterView()
		{
			InitializeComponent();
			Presenter = new TextConverterPresenter(new TextConverterModel(), this);
		}

		private void ButtonTranslit_OnClick(object sender, RoutedEventArgs e)
		{
			Presenter.OnButtonTranslitClicked();
		}

		public void DuplicateTextRange()
		{
			MemoryStream memoryStream = new MemoryStream();
			TextRange cyryllictextRange = new TextRange(RichTextBoxCyryllic.Document.ContentStart, RichTextBoxCyryllic.Document.ContentEnd);
			TextRange latinTextRange = new TextRange(RichTextBoxLatin.Document.ContentStart, RichTextBoxLatin.Document.ContentEnd);

			cyryllictextRange.Save(memoryStream, DataFormats.Rtf);
			latinTextRange.Load(memoryStream, DataFormats.Rtf);
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

				Clipboard.SetData(DataFormats.Rtf, Encoding.UTF8.GetString(((MemoryStream)stream).ToArray()));
			}
		}

		private void ToggleButtonRealTimeTransliteration_OnClick(object sender, RoutedEventArgs e)
		{
			if (ToggleButtonRealTimeTransliteration.IsChecked == true)
			{
				RichTextBoxCyryllic.TextChanged += ButtonTranslit_OnClick;
			}
			else
			{
				RichTextBoxCyryllic.TextChanged -= ButtonTranslit_OnClick;
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

		public TextRange GetTextRange()
		{
			return new TextRange(RichTextBoxLatin.Document.ContentStart, RichTextBoxLatin.Document.ContentEnd);
		}
	}
}
