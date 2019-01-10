using System.Threading.Tasks;
using Translit.Models.Pages;
using Translit.Views.Pages;

namespace Translit.Presenters.Pages
{
	public class WordsEditorPresenter : IWordsEditorPresenter
	{
		private IWordsEditorModel Model { get; }
		private IWordsEditorView View { get; }
		public WordsEditorPresenter(WordsEditorView view)
		{
			Model = new WordsEditorModel();
			View = view;
		}

		public void OnPageLoaded()
		{
			UpdateWords();
		}

		public async void AddNewWord(string cyryllic, string latin)
		{
			if (!Model.CheckWordsLength(cyryllic, latin))
			{
				View.ShowNotification("SnackBarNotAllowedLength");
				return;
			}

			await Task.Factory.StartNew(() =>
			{
				Model.AddWord(cyryllic, latin).Wait();
			});

			switch (Model.ReasonPhrase)
			{
				case "Created":
					View.ShowNotification("SnackBarRecordSuccessfullyAdded");
					break;
				case "InternalServerError":
					View.ShowNotification("SnackBarServerSideError");
					break;
				case "Conflict":
					View.ShowNotification("SnackBarTheWordIsAlreadyInTheDatabase");
					break;
				default:
					View.ShowNotification("SnackBarError");
					break;
			}

			UpdateWords();
		}

		public async void EditWord(int id, string cyryllic, string latin)
		{
			if (!Model.CheckWordsLength(cyryllic, latin))
			{
				View.ShowNotification("SnackBarNotAllowedLength");
				return;
			}
			await Task.Factory.StartNew(() =>
			{
				Model.EditWord(id, cyryllic, latin).Wait();
			});

			switch (Model.ReasonPhrase)
			{
				case "OK":
					View.ShowNotification("SnackBarRecordEdited");
					break;
				case "InternalServerError":
					View.ShowNotification("SnackBarServerSideError");
					break;
				case "Conflict":
					View.ShowNotification("SnackBarTheWordIsAlreadyInTheDatabase");
					break;
				default:
					View.ShowNotification("SnackBarError");
					break;
			}

			UpdateWords();
		}

		public async void DeleteWord(int id)
		{
			await Task.Factory.StartNew(() => { Model.DeleteWord(id); });

			switch (Model.ReasonPhrase)
			{
				case "OK":
					View.ShowNotification("SnackBarWordDeleted");
					break;
				case "InternalServerError":
					View.ShowNotification("SnackBarServerSideError");
					break;
				default:
					View.ShowNotification("SnackBarError");
					break;
			}

			UpdateWords();
		}

		public void UpdateWords()
		{
			View.UpdateWords(Model.GetWordsFromDatabase());
		}
	}
}
