using System.ComponentModel;
using System.Threading.Tasks;
using Translit.Models.Pages;
using Translit.Views.Pages;

namespace Translit.Presenters.Pages
{
	public class DatabasePresenter : IDatabasePresenter
	{
		private IDatabaseModel Model { get; }
		private IDatabaseView View { get; }

		public DatabasePresenter(DatabaseView view)
		{
			Model = new DatabaseModel();
			View = view;
		}

		public async void DownloadDatabase()
		{
			await Task.Factory.StartNew(() =>
			{
				View.ToggleUpdateButtonState(false);
				if (!Model.DownloadDatabaseFromServer())
				{
					View.ToggleUpdateButtonState(true);
					View.ShowNotification("SnackBarBadInternetConnection");
					return;
				}
				View.ToggleProgressBarVisibility();
				Model.DeleteOldDatabase();
				Model.PropertyChanged += TrackProperties;
				Model.InsertData();
				View.ShowNotification("SnackBarUpdateCompleted");
				View.SetInfoAboutDatabase(Model.GetInfoAboutDatabase());
				View.ToggleUpdateButtonState(true);
				View.ToggleProgressBarVisibility();
			});
		}

		public void TrackProperties(object sender, PropertyChangedEventArgs e)
		{
			var a = ((DatabaseModel)sender).PercentOfSymbols;
			var b = ((DatabaseModel)sender).PercentOfExceptions;

			View.UpdateProgressValues(a, b);
		}

		public void OnPageLoaded()
		{
			View.SetInfoAboutDatabase(Model.GetInfoAboutDatabase());
		}
	}
}
