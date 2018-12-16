using System.Windows.Documents;
using Translit.Models.Pages;
using Translit.Views.Pages;

namespace Translit.Presenters.Pages
{
	public class TextConverterPresenter
	{
		public TextConverterModel Model { get;}
		public TextConverterView View { get;}

		public TextConverterPresenter(TextConverterModel model, TextConverterView view)
		{
			Model = model;
			View = view;
		}

		public void OnButtonTranslitClicked()
		{
			View.DuplicateTextRange();
			Model.TranslitTextRange(View.GetTextRange());
		}

		public void OnRichTextBoxCyryllicTextChanged(TextRange textRange)
		{
			Model.LatinTextRange = textRange;
		}

		public void OnButtonCopyClicked()
		{
			Model.CopyToClipboard(View.GetTextRange());
		}
	}
}