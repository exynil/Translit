﻿using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;
using Translit.Models.Other;
using Translit.Models.Pages;

namespace Translit.ViewModels.Pages
{
    internal class DatabaseViewModel : INotifyPropertyChanged
    {
        private bool _canUpdate;
        private string _database;
        private string _databaseSize;
        private bool _isIndeterminate;
        private string _lastUpdate;
        private int _percentOfSymbols;
        private int _percentOfWords;
        private Visibility _progressBarVisibility;
        private string _symbolsCount;
        private string _symbolsInserting;
        private string _updateButtonContent;
        private string _wordsCount;
        private string _wordsInserting;

        public DatabaseViewModel()
        {
            Model = new DatabaseModel();
            Model.PropertyChanged += OnModelPropertyChanged;
            MessageQueue = new SnackbarMessageQueue();
            ProgressBarVisibility = Visibility.Hidden;
            CanUpdate = true;
            IsIndeterminate = false;
            SetInfoAboutDatabase();
            UpdateButtonContent = GetRes(Model.CollectionExists() ? "ButtonUpdate" : "ButtonDownload");
        }

        public DatabaseModel Model { get; set; }
        public SnackbarMessageQueue MessageQueue { get; set; }

        public string Database
        {
            get => _database;
            set
            {
                _database = value;
                OnPropertyChanged();
            }
        }

        public string DatabaseSize
        {
            get => _databaseSize;
            set
            {
                _databaseSize = value;
                OnPropertyChanged();
            }
        }

        public string SymbolsCount
        {
            get => _symbolsCount;
            set
            {
                _symbolsCount = value;
                OnPropertyChanged();
            }
        }

        public string WordsCount
        {
            get => _wordsCount;
            set
            {
                _wordsCount = value;
                OnPropertyChanged();
            }
        }

        public string LastUpdate
        {
            get => _lastUpdate;
            set
            {
                _lastUpdate = value;
                OnPropertyChanged();
            }
        }

        public Visibility ProgressBarVisibility
        {
            get => _progressBarVisibility;
            set
            {
                _progressBarVisibility = value;
                OnPropertyChanged();
            }
        }

        public string SymbolsInserting
        {
            get => _symbolsInserting;
            set
            {
                _symbolsInserting = value;
                OnPropertyChanged();
            }
        }

        public int PercentOfSymbols
        {
            get => _percentOfSymbols;
            set
            {
                _percentOfSymbols = value;
                OnPropertyChanged();
            }
        }

        public string WordsInserting
        {
            get => _wordsInserting;
            set
            {
                _wordsInserting = value;
                OnPropertyChanged();
            }
        }

        public int PercentOfWords
        {
            get => _percentOfWords;
            set
            {
                _percentOfWords = value;
                OnPropertyChanged();
            }
        }

        public bool CanUpdate
        {
            get => _canUpdate;
            set
            {
                _canUpdate = value;
                OnPropertyChanged();
            }
        }

        public bool IsIndeterminate
        {
            get => _isIndeterminate;
            set
            {
                _isIndeterminate = value;
                OnPropertyChanged();
            }
        }

        public string UpdateButtonContent
        {
            get => _updateButtonContent;
            set
            {
                _updateButtonContent = value;
                OnPropertyChanged();
            }
        }

        public ICommand Update
        {
            get
            {
                return new DelegateCommand(o =>
                {
                    Task.Factory.StartNew(() =>
                    {
                        CanUpdate = false;
                        IsIndeterminate = true;
                        UpdateButtonContent = GetRes("ButtonLoading");
                        if (Model.DownloadOrUpdateDatabase())
                        {
                            Analytics.Start();
                            ProgressBarVisibility = Visibility.Visible;
                            Model.InsertData();
                            MessageQueue.Enqueue(GetRes("SnackBarUpdateCompleted"));
                            SetInfoAboutDatabase();
                            ProgressBarVisibility = Visibility.Hidden;
                            UpdateButtonContent = GetRes("ButtonUpdate");
                            IsIndeterminate = false;
                            CanUpdate = true;
                        }
                        else
                        {
                            IsIndeterminate = false;
                            CanUpdate = true;
                            UpdateButtonContent = GetRes(Model.CollectionExists() ? "ButtonUpdate" : "ButtonDownload");
                            MessageQueue.Enqueue(GetRes("SnackBarBadInternetConnection"));
                        }
                    });
                });
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PercentOfSymbols = Model.PercentOfSymbols;
            PercentOfWords = Model.PercentOfWords;
            SymbolsInserting = GetRes("TextBlockCharacterLoading") + ": " + PercentOfSymbols + "%";
            WordsInserting = GetRes("TextBlockLoadingWords") + ": " + PercentOfWords + "%";
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string GetRes(string key)
        {
            return Application.Current.Resources[key].ToString();
        }

        private void SetInfoAboutDatabase()
        {
            if (Model.CollectionExists())
            {
                Database = GetRes("TextBlockDatabase");
                DatabaseSize = $"{GetRes("TextBlockDatabaseSize")} {Model.GetDatabaseSize() / 1024} KB";
                SymbolsCount = $"{GetRes("TextBlockAmountOfCharacters")} {Model.GetSymbolCount()}";
                WordsCount = $"{GetRes("TextBlockAmountOfWords")} {Model.GetWordCount()}";
                LastUpdate =
                    $"{GetRes("TextBlockLastUpdate")} {Model.GetLastUpdateTime().ToString(CultureInfo.CurrentCulture)}";
            }
            else
            {
                Database = GetRes("TextBlockDatabaseNotFound");
            }
        }
    }
}