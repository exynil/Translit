using System.ComponentModel;
using Translit.Entity;

namespace Translit.Models.Pages
{
	interface IDatabaseModel
	{
		bool DownloadDatabaseFromServer();
		void DeleteOldDatabase();
		event PropertyChangedEventHandler PropertyChanged;
		void InsertData();
	}
}
