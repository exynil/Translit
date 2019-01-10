using System.Windows.Documents;
using Translit.Models.Pages;
using Translit.Views.Pages;

namespace Translit.Presenters.Pages
{
	public class TextConverterPresenter : ITextConverterPresenter
	{
		private ITextConverterModel Model { get;}
		private ITextConverterView View { get;}

		public TextConverterPresenter(TextConverterView view)
		{
			Model = new TextConverterModel();
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