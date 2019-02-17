using System.ComponentModel;

namespace Translit.Models.Pages
{
    internal interface IFileConverterModel
    {
        event PropertyChangedEventHandler PropertyChanged;
        bool TranslitFiles(string[] files, bool? ignoreSelectedText);
    }
}