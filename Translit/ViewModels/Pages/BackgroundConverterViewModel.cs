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

        public BackgroundConverterViewModel()
        {
            Model = new BackgroundConverterModel();
            MessageQueue = new SnackbarMessageQueue();
            ActivatorContent = GetRes(Model.IsTransliteratorEnabled ? "ButtonDeactivate" : "ButtonActivate");
        }

        public BackgroundConverterModel Model { get; set; }
        public SnackbarMessageQueue MessageQueue { get; set; }

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
                    else
                    {
                        Model.Subscribe();
                        ActivatorContent = GetRes("ButtonDeactivate");
                        MessageQueue.Enqueue(GetRes("SnackBarBackgroundTransliteratorActivated"));
                    }
                });
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

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