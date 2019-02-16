using MaterialDesignThemes.Wpf;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Translit.Models.Other;

namespace Translit.ViewModels.Pages
{
    class StatisticsViewModel : INotifyPropertyChanged
    {
        public SnackbarMessageQueue MessageQueue { get; set; }

        public Dictionary<string, string> Statistics
        {
            get => _statistics;
            set
            {
                _statistics = value;
                OnPropertyChanged();
            }
        }
        private Dictionary<string, string> _statistics;

        public event PropertyChangedEventHandler PropertyChanged;

        public StatisticsViewModel()
        {
            MessageQueue = new SnackbarMessageQueue();

            Statistics = new Dictionary<string, string>();

            if (Update.CanExecute(null))
            {
                Update.Execute(null);
            }   
         
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ICommand Update
        {
            get
            {
                return new DelegateCommand(o =>
                {
                    var counter = new FileCounter();

                    counter.Add(Analytics.LocalUserData.Counter);
                    counter.Add(Analytics.UnsentUserData.Counter);

                    Statistics.Clear();

                    Statistics.Add(GetRes("TextBlockWordDocumentsWithoutExtensions"), counter.Word.ToString());
                    Statistics.Add(GetRes("TextBlockExcelSpreadsheetsWithoutExtensions"), counter.Excel.ToString());
                    Statistics.Add(GetRes("TextBlockPowerPointPresentationsWithoutExtensions"), counter.PowerPoint.ToString());
                    Statistics.Add(GetRes("TextBlockPdfDocumentsWithoutExtensions"), counter.Pdf.ToString());
                    Statistics.Add(GetRes("TextBlockRtfDocumentsWithoutExtensions"), counter.Rtf.ToString());
                    Statistics.Add(GetRes("TextBlockTextDocumentsWithoutExtensions"), counter.Txt.ToString());
                    Statistics.Add(GetRes("TextBlockTotal"), counter.GetSum().ToString());

                    MessageQueue.Enqueue(GetRes("SnackBarStatisticsUpdated"));
                });
            }
        }

        private string GetRes(string key)
        {
            return Application.Current.Resources[key].ToString();
        }
    }
}
