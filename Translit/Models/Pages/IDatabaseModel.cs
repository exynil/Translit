using System.ComponentModel;

namespace Translit.Models.Pages
{
    interface IDatabaseModel
	{
		bool DownloadDatabaseFromServer();
		event PropertyChangedEventHandler PropertyChanged;
		void InsertData();
	}
}
