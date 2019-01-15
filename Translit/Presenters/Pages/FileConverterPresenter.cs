using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Translit.Models.Pages;
using Translit.Views.Pages;

namespace Translit.Presenters.Pages
{
	public class FileConverterPresenter : IFileConverterPresenter
	{
		private IFileConverterModel Model { get;}
		private IFileConverterView View { get;}

		public FileConverterPresenter(FileConverterView view)
		{
			Model = new FileConverterModel();
			View = view;
		}

		public async void TranslitFiles(string[] files, bool? ignoreSelectedText)
		{
			if (files.Length == 0) return;

			View.BlockUnlockButtons();

			View.ToggleProgressBarVisibility();

			View.SetProgressBarStartValues(files.Length - 1);

			await Task.Factory.StartNew(() =>
			{
				Model.PropertyChanged += TrackProperties;
				Model.TranslitFiles(files, ignoreSelectedText);
			});

			View.ToggleProgressBarVisibility();

			View.BlockUnlockButtons();

			View.ShowNotification("SnackBarTransliterationCompleted");
		}

		public void TrackProperties(object sender, PropertyChangedEventArgs e)
		{
			var a = ((FileConverterModel)sender).NumberOfDocumentsTranslated;
			var b = ((FileConverterModel)sender).NumberOfDocuments;
			var c = ((FileConverterModel)sender).PercentOfExceptions;
			var d = ((FileConverterModel)sender).PercentOfSymbols;
			View.UpdateProgressValues(a, b, c, d);
		}
	}
}
