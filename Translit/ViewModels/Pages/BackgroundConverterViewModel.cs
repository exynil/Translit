using MaterialDesignThemes.Wpf;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Translit.Models.Other;
using Translit.Models.Pages;
using Application = System.Windows.Application;

namespace Translit.ViewModels.Pages
{
    class BackgroundConverterViewModel : INotifyPropertyChanged
    {
        private string _activatorContent;
        public BackgroundConverterModel Model { get; set; }
        public SnackbarMessageQueue MessageQueue { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        public string ActivatorContent
        {
            get => _activatorContent;
            set
            {
                _activatorContent = value;
                OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public BackgroundConverterViewModel()
        {
            Model = new BackgroundConverterModel();
            MessageQueue = new SnackbarMessageQueue();
            ActivatorContent = GetRes(Model.IsTransliteratorEnabled ? "ButtonDeactivate" : "ButtonActivate");
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

        private string GetRes(string key)
        {
            return Application.Current.Resources[key].ToString();
        }
    }
}
