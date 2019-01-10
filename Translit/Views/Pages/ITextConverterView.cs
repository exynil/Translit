using System.Windows.Documents;

namespace Translit.Views.Pages
{
	interface ITextConverterView
	{
		void DuplicateTextRange();
		TextRange GetTextRange();
	}
}
