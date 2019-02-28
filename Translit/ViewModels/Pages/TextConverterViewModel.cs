using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;
using Translit.Models.Other;
using Translit.Models.Pages;
using Translit.Properties;

namespace Translit.ViewModels.Pages
{
    internal class TextConverterViewModel : INotifyPropertyChanged
    {
        private string _cyryllic;
        private double _fontSize;
        private string _latin;

        public TextConverterViewModel()
        {
            Model = new TextConverterModel();
            MessageQueue = new SnackbarMessageQueue();
            FontSize = Settings.Default.TextConverterFontSize;
        }

        public TextConverterModel Model { get; set; }
        public SnackbarMessageQueue MessageQueue { get; set; }

        public string Cyryllic
        {
            get => _cyryllic;
            set
            {
                _cyryllic = value;
                if (Model.CollectionExists())
                    Transliterate();
                else
                    MessageQueue.Enqueue(GetRes("SnackBarDatabaseNotFound"));
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

        public double FontSize
        {
            get => _fontSize;
            set
            {
                _fontSize = value;
                Settings.Default.TextConverterFontSize = value;
                OnPropertyChanged();
            }
        }

        public ICommand Clear
        {
            get { return new DelegateCommand(o => { Cyryllic = Latin = ""; }); }
        }

        public ICommand Copy
        {
            get { return new DelegateCommand(o => { Clipboard.SetData(DataFormats.UnicodeText, Latin); }); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Transliterate()
        {
            Task.Factory.StartNew(
                () => { Latin = Model.Transliterate(Cyryllic) ?? GetRes("SnackBarDatabaseNotFound"); });
        }

        // Получение ресурса по ключу
        public string GetRes(string key)
        {
            return Application.Current.Resources[key].ToString();
        }
    }
}