using System.ComponentModel;

namespace Translit.Models.Pages
{
    internal interface IDatabaseModel
    {
        bool DownloadOrUpdateDatabase();
        event PropertyChangedEventHandler PropertyChanged;
        void InsertData();
    }
}