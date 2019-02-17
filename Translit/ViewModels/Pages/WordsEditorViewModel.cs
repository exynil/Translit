using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;
using Translit.Models.Other;
using Translit.Models.Pages;
using Translit.Properties;
using Translit.Views.DialogWindows;

namespace Translit.ViewModels.Pages
{
    internal class WordsEditorViewModel : INotifyPropertyChanged
    {
        private Visibility _controlsVisibility;
        private Word _selectedWord;
        private ObservableCollection<Word> _words;

        public WordsEditorViewModel()
        {
            Model = new WordsEditorModel();
            MessageQueue = new SnackbarMessageQueue();

            Words = Model.GetWordsFromDatabase();

            if (Words == null) MessageQueue.Enqueue(GetRes("SnackBarDatabaseNotFound"));

            ControlsVisibility = Settings.Default.PermissionToChange ? Visibility.Visible : Visibility.Collapsed;
        }

        public WordsEditorModel Model { get; set; }
        public SnackbarMessageQueue MessageQueue { get; set; }

        public ObservableCollection<Word> Words
        {
            get => _words;
            set
            {
                _words = value;
                OnPropertyChanged();
            }
        }

        public Word SelectedWord
        {
            get => _selectedWord;
            set
            {
                _selectedWord = value;
                OnPropertyChanged();
            }
        }

        public Visibility ControlsVisibility
        {
            get => _controlsVisibility;
            set
            {
                _controlsVisibility = value;
                OnPropertyChanged();
            }
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

                        if (!Model.CheckWordsLength(cyryllic, latin))
                        {
                            MessageQueue.Enqueue(GetRes("SnackBarNotAllowedLength"));
                            return;
                        }

                        Task.Factory.StartNew(() =>
                        {
                            Model.AddWord(cyryllic, latin).Wait();

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

                            if (Update.CanExecute(null)) Update.Execute(null);
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
                    if (SelectedWord == null)
                    {
                        MessageQueue.Enqueue(GetRes("SnackBarHighlightAnEntryToEditOrDelete"));
                        return;
                    }

                    var id = SelectedWord.Id;
                    var cyryllic = SelectedWord.Cyryllic;
                    var latin = SelectedWord.Latin;

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

                        if (!Model.CheckWordsLength(cyryllic, latin))
                        {
                            MessageQueue.Enqueue(GetRes("SnackBarNotAllowedLength"));
                            return;
                        }

                        Task.Factory.StartNew(() =>
                        {
                            Model.EditWord(id, cyryllic, latin).Wait();

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

                            if (Update.CanExecute(null)) Update.Execute(null);
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
                    if (SelectedWord == null)
                    {
                        MessageQueue.Enqueue(GetRes("SnackBarHighlightAnEntryToEditOrDelete"));
                        return;
                    }

                    // Получаем подтверждение пользователя
                    // Получаем подтверждение пользователя
                    var questionView = new QuestionView(GetRes("TextBlockDoYouReallyWantToDelete"));

                    if (questionView.ShowDialog() == true)
                        Task.Factory.StartNew(() =>
                        {
                            Model.DeleteWord(SelectedWord.Id);

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

                            if (Update.CanExecute(null)) Update.Execute(null);
                        });
                    else
                        MessageQueue.Enqueue(GetRes("SnackBarDeleteCanceled"));
                });
            }
        }

        public ICommand Update
        {
            get { return new DelegateCommand(o => { Words = Model.GetWordsFromDatabase(); }); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Получение ресурса по ключу
        public string GetRes(string key)
        {
            return Application.Current.Resources[key].ToString();
        }
    }
}