using System.ComponentModel;

namespace Translit.Models.Pages
{
	interface IFileConverterModel
	{
		event PropertyChangedEventHandler PropertyChanged;
		void TranslitFiles(string[] files, bool? ignoreSelectedText);
	}
}
