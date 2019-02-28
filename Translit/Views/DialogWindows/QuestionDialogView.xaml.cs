using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Translit.Models.Other;

namespace Translit.Views.DialogWindows
{
    public partial class QuestionDialogView : INotifyPropertyChanged
    {
        private string _question;

        public QuestionDialogView(string question)
        {
            InitializeComponent();
            DataContext = this;
            Owner = Application.Current.MainWindow;
            Question = question;
        }

        public string Question
        {
            get => _question;
            set
            {
                _question = value;
                OnPropertyChanged();
            }
        }

        public ICommand Accept
        {
            get
            {
                return new DelegateCommand(o =>
                {
                    DialogResult = true;
                    Close();
                });
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}