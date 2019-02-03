using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;
using Translit.Entity;
using Translit.Models.Pages;
using Translit.Properties;
using Translit.Views.DialogWindows;

namespace Translit.ViewModels.Pages
{
	class SymbolsEditorViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		public SymbolsEditorModel Model { get; set; }
		public SnackbarMessageQueue MessageQueue { get; set; }
		public ObservableCollection<Symbol> Symbols { get; set; }
		public Symbol SelectedSymbol { get; set; }
		public Visibility ControlsVisibility { get; set; }

		public SymbolsEditorViewModel()
		{
			Model = new SymbolsEditorModel();
			MessageQueue = new SnackbarMessageQueue();

			Symbols = Model.GetSymbolsFromDatabase();

			if (Symbols == null)
			{
				MessageQueue.Enqueue(GetRes("SnackBarDatabaseNotFound"));
			}

			ControlsVisibility = Settings.Default.IsUserAuthorized ? Visibility.Visible : Visibility.Collapsed;
		}

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public ICommand Add
		{
			get
			{
				return new DelegateCommand(o =>
				{
					//Создаем диалоговое окно
					var addDialogView = new AddDialogView();

					// Ожидаем ответ пользователя
					if (addDialogView.ShowDialog() == true)
					{
						var cyryllic = addDialogView.TextBoxAddCyryllic.Text;
						var latin = addDialogView.TextBoxAddLatin.Text;

                        // Закрываем диалоговое окно
					    addDialogView.Close();

						if (!Model.CheckSymbolsLength(cyryllic, latin))
						{
							MessageQueue.Enqueue(GetRes("SnackBarNotAllowedLength"));
							return;
						}

						Task.Factory.StartNew(() =>
						{
							Model.AddSymbol(cyryllic, latin).Wait();

							switch (Model.ReasonPhrase)
							{
								case "Created":
									MessageQueue.Enqueue(GetRes("SnackBarRecordSuccessfullyAdded"));
									break;
								case "InternalServerError":
									MessageQueue.Enqueue(GetRes("SnackBarServerSideError"));
									break;
								case "Conflict":
									MessageQueue.Enqueue(GetRes("SnackBarTheWordIsAlreadyInTheDatabase"));
									break;
								default:
									MessageQueue.Enqueue(GetRes("SnackBarError"));
									break;
							}

							if (Update.CanExecute(null))
							{
								Update.Execute(null);
							}
						});
					}
					else
					{
						MessageQueue.Enqueue(GetRes("SnackBarAddingEntryCanceled"));
					}
				});
			}
		}

		public ICommand Edit
		{
			get
			{
				return new DelegateCommand(o =>
				{
					if (SelectedSymbol == null)
					{
						MessageQueue.Enqueue(GetRes("SnackBarHighlightAnEntryToEditOrDelete"));
						return;
					}

					var id = SelectedSymbol.Id;
					var cyryllic = SelectedSymbol.Cyryllic;
					var latin = SelectedSymbol.Latin;

					// Создаем диалоговое окно
					var editDialogView = new EditDialogView(cyryllic, latin);

					// Ожидаем ответа пользователя
					if (editDialogView.ShowDialog() == true)
					{
						// Получаем измененные данные
						var cyryllicModified = editDialogView.TextBoxEditCyryllic.Text;
						var latinModified = editDialogView.TextBoxEditLatin.Text;

                        // Закрываем диалоговое окно
					    editDialogView.Close();

						cyryllic = cyryllic != cyryllicModified ? cyryllicModified : null;

						latin = latin != latinModified ? latinModified : null;

						if (!Model.CheckSymbolsLength(cyryllic, latin))
						{
							MessageQueue.Enqueue(GetRes("SnackBarNotAllowedLength"));
							return;
						}

						Task.Factory.StartNew(() =>
						{
							Model.EditSymbol(id, cyryllic, latin).Wait();

							switch (Model.ReasonPhrase)
							{
								case "OK":
									MessageQueue.Enqueue(GetRes("SnackBarRecordEdited"));
									break;
								case "InternalServerError":
									MessageQueue.Enqueue(GetRes("SnackBarServerSideError"));
									break;
								case "Conflict":
									MessageQueue.Enqueue(GetRes("SnackBarTheWordIsAlreadyInTheDatabase"));
									break;
								default:
									MessageQueue.Enqueue(GetRes("SnackBarError"));
									break;
							}

							if (Update.CanExecute(null))
							{
								Update.Execute(null);
							}
						});
					}
					else
					{
						MessageQueue.Enqueue(GetRes("SnackBarRecordEditingCanceled"));
					}
				});
			}
		}

		public ICommand Delete
		{
			get
			{
				return new DelegateCommand(o =>
				{
					if (SelectedSymbol == null)
					{
						MessageQueue.Enqueue(GetRes("SnackBarHighlightAnEntryToEditOrDelete"));
						return;
					}

					// Получаем подтверждение пользователя
					var questionView = new QuestionView(GetRes("TextBlockDoYouReallyWantToDelete"));

					if (questionView.ShowDialog() == true)
					{
						Task.Factory.StartNew(() =>
						{
							Model.DeleteSymbol(SelectedSymbol.Id);

							switch (Model.ReasonPhrase)
							{
								case "OK":
									MessageQueue.Enqueue(GetRes("SnackBarWordDeleted"));
									break;
								case "InternalServerError":
									MessageQueue.Enqueue(GetRes("SnackBarServerSideError"));
									break;
								default:
									MessageQueue.Enqueue(GetRes("SnackBarError"));
									break;
							}

							if (Update.CanExecute(null))
							{
								Update.Execute(null);
							}
						});
					}
					else
					{
						MessageQueue.Enqueue(GetRes("SnackBarDeleteCanceled"));
					}
				});
			}
		}

		public ICommand Update
		{
			get
			{
				return new DelegateCommand(o => { Symbols = Model.GetSymbolsFromDatabase(); });
			}
		}

		// Получение ресурса по ключу
		public string GetRes(string key)
		{
			return Application.Current.Resources[key].ToString();
		}
	}
}
