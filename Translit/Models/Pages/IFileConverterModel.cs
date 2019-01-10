using System.ComponentModel;

namespace Translit.Models.Pages
{
	interface IFileConverterModel
	{
		string SelectFile();
		event PropertyChangedEventHandler PropertyChanged;
		void TranslitFile(string filename);
		string[] SelectFolder();
		void TranslitFiles(string[] files);
	}
}
