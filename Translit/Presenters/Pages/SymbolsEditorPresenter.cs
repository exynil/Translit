using System.Threading.Tasks;
using Translit.Models.Pages;
using Translit.Views.Pages;

namespace Translit.Presenters.Pages
{
	public class SymbolsEditorPresenter
	{
		public SymbolsEditorModel Model { get; }
		public SymbolsEditorView View { get; }
        public SymbolsEditorPresenter(SymbolsEditorModel model, SymbolsEditorView view)
        {
            Model = model;
            View = view;
        }

        public void OnPageLoaded()
        {
			UpdateSymbols();
		}

        public async void AddNewSymbol(string cyryllic, string latin)
        {
            if (!Model.CheckSymbolsLength(cyryllic, latin))
            {
                View.ShowNotification("SnackBarNotAllowedLength");
                return;
            }

            await Task.Factory.StartNew(() =>
            {
                Model.AddSymbol(cyryllic, latin).Wait();
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

			UpdateSymbols();
		}

	    public async void EditSymbol(int id, string cyryllic, string latin)
	    {
		    if (!Model.CheckSymbolsLength(cyryllic, latin))
		    {
				View.ShowNotification("SnackBarNotAllowedLength");
			    return;
		    }
			await Task.Factory.StartNew(() =>
		    {
			    Model.EditSymbol(id, cyryllic, latin).Wait();
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

		    UpdateSymbols();
	    }

	    public async void DeleteSymbol(int id)
	    {
			await Task.Factory.StartNew(() => { Model.DeleteSymbol(id); });

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

		    UpdateSymbols();
		}

		public void UpdateSymbols()
	    {
			View.UpdateSymbols(Model.GetSymbolsFromDatabase());
	    }
    }
}
