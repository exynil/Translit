using System.ComponentModel;

namespace Translit.Models.Pages
{
	interface IFileConverterModel
	{
		event PropertyChangedEventHandler PropertyChanged;
		void TranslitFile(string filename, bool? ignoreMarkers);
		void TranslitFiles(string[] files, bool? ignoreMarkers);
	}
}
