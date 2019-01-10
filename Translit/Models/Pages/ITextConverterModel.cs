using System.Windows.Documents;

namespace Translit.Models.Pages
{
	interface ITextConverterModel
	{
		void TranslitTextRange(TextRange textRange);
		TextRange LatinTextRange { get; set; }
		void CopyToClipboard(TextRange textRange);
	}
}
