using System.ComponentModel;

namespace Translit.Models.Pages
{
	interface IFileConverterModel
	{
		event PropertyChangedEventHandler PropertyChanged;
		void TranslitFile(string filename);
		void TranslitFiles(string[] files);
	}
}
