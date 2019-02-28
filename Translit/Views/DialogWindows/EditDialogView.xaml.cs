using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Translit.Models.Other;

namespace Translit.Views.DialogWindows
{
    public partial class EditDialogView : INotifyPropertyChanged
    {
        private string _cyryllic;
        private string _latin;
        private string _message;

        public EditDialogView(string cyryllic, string latin)
        {
            InitializeComponent();
            DataContext = this;
            Owner = Application.Current.MainWindow;
            Cyryllic = cyryllic;
            Latin = latin;
        }

        public string Cyryllic
        {
            get => _cyryllic;
            set
            {
                _cyryllic = value;
                OnPropertyChanged();
            }
        }

        public string Latin
        {
            get => _latin;
            set
            {
                _latin = value;
                OnPropertyChanged();
            }
        }

        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                OnPropertyChanged();
            }
        }

        public ICommand Accept
        {
            get
            {
                return new DelegateCommand(o =>
                {
                    if (Cyryllic != string.Empty && Latin != string.Empty)
                        DialogResult = true;
                    else
                        Message = GetRes("TextBlockPleaseFillInAllFields");
                });
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // Получение ресурса по ключу
        public string GetRes(string key)
        {
            return Application.Current.Resources[key].ToString();
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}