using MaterialDesignThemes.Wpf;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Translit.Models.Other;

namespace Translit.ViewModels.Pages
{
    internal class StatisticsViewModel : INotifyPropertyChanged
    {
        public StatisticsModel Model { get; set; }
        private Dictionary<string, string> _statistics;

        public StatisticsViewModel()
        {
            Model = new StatisticsModel();
            MessageQueue = new SnackbarMessageQueue();

            Statistics = new Dictionary<string, string>();

            UpdateStatistics();
        }

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

        public ICommand Update
        {
            get
            {
                return new DelegateCommand(o =>
                {
                    UpdateStatistics();

                    MessageQueue.Enqueue(GetRes("SnackBarStatisticsUpdated"));
                });
            }
        }

        public void UpdateStatistics()
        {
            var counter = new FileCounter();
            
            counter.Add(Model.GetLocalUserData().Counter);

            counter.Add(Model.GetUnsentUserData().Counter);

            Statistics = new Dictionary<string, string>
            {
                [GetRes("TextBlockWordDocuments")] = counter.Word.ToString(),
                [GetRes("TextBlockExcelSpreadsheets")] = counter.Excel.ToString(),
                [GetRes("TextBlockPowerPointPresentations")] = counter.PowerPoint.ToString(),
                [GetRes("TextBlockPdfDocuments")] = counter.Pdf.ToString(),
                [GetRes("TextBlockRtfDocuments")] = counter.Rtf.ToString(),
                [GetRes("TextBlockTextDocuments")] = counter.Txt.ToString(),
                [GetRes("TextBlockTotal")] = counter.GetSum().ToString()
            };

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