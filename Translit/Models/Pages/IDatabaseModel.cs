using System.ComponentModel;

namespace Translit.Models.Pages
{
    interface IDatabaseModel
	{
		bool DownloadOrUpdateDatabase();
		event PropertyChangedEventHandler PropertyChanged;
		void InsertData();
	}
}
