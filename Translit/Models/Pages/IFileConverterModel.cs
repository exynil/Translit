using System.ComponentModel;

namespace Translit.Models.Pages
{
	interface IFileConverterModel
	{
		event PropertyChangedEventHandler PropertyChanged;
		bool TranslitFiles(string[] files, bool? ignoreSelectedText);
	}
}
