using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;
using Translit.Models.Other;
using Translit.Models.Pages;

namespace Translit.ViewModels.Pages
{
    internal class BackgroundConverterViewModel : INotifyPropertyChanged
    {
        private string _activatorContent;
        private string _inputText;

        public BackgroundConverterViewModel()
        {
            Model = new BackgroundConverterModel();
            Model.PropertyChanged += OnModelPropertyChanged;
            MessageQueue = new SnackbarMessageQueue();
            ActivatorContent = GetRes(Model.IsTransliteratorEnabled ? "ButtonDeactivate" : "ButtonActivate");
        }

        public BackgroundConverterModel Model { get; set; }
        public SnackbarMessageQueue MessageQueue { get; set; }

        public string InputText
        {
            get => _inputText;
            set
            {
                _inputText = value;
                OnPropertyChanged();
            }
        }

        public string ActivatorContent
        {
            get => _activatorContent;
            set
            {
                _activatorContent = value;
                OnPropertyChanged();
            }
        }

        public ICommand ActivateOrDeactivate
        {
            get
            {
                return new DelegateCommand(o =>
                {
                    if (Model.IsTransliteratorEnabled)
                    {
                        Model.Unsubscribe();
                        ActivatorContent = GetRes("ButtonActivate");
                        MessageQueue.Enqueue(GetRes("SnackBarBackgroundTransliteratorDeactivated"));
                    }
                    else if (Model.CollectionExists())
                    {
                        Model.Subscribe();
                        ActivatorContent = GetRes("ButtonDeactivate");
                        MessageQueue.Enqueue(GetRes("SnackBarBackgroundTransliteratorActivated"));
                    }
                    else
                    {
                        MessageQueue.Enqueue(GetRes("SnackBarDatabaseNotFound"));
                    }
                });
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            InputText = Model.InputText.ToString();
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string GetRes(string key)
        {
            return Application.Current.Resources[key].ToString();
        }
    }
}