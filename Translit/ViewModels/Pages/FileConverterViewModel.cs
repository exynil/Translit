﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using MaterialDesignThemes.Wpf;
using Translit.Models.Other;
using Translit.Models.Pages;
using Translit.Properties;
using Application = System.Windows.Application;
using DataFormats = System.Windows.DataFormats;
using IDataObject = System.Windows.IDataObject;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace Translit.ViewModels.Pages
{
    internal class FileConverterViewModel : INotifyPropertyChanged
    {
        private string _errors;
        private string _fileName;
        private bool _ignoreSelectedText;
        private string _left;
        private int _numberOfFiles;
        private int _numberOfTransliteratedFiles;
        private int _percentOfSymbols;
        private int _percentOfWords;
        private ICommand _previewDropCommand;
        private Visibility _progressBarVisibility;
        private string _symbolProgress;
        private string _timeSpent;
        private string _wordProgress;

        public FileConverterViewModel()
        {
            Model = new FileConverterModel();
            Model.PropertyChanged += OnModelPropertyChanged;
            MessageQueue = new SnackbarMessageQueue();
            Timer = new DispatcherTimer();
            Timer.Tick += UpdateTimeSpent;
            Timer.Interval = TimeSpan.FromMilliseconds(1);
            Stopwatch = new Stopwatch();
            ProgressBarVisibility = Visibility.Hidden;
            FileName = "";
        }

        public FileConverterModel Model { get; set; }
        public SnackbarMessageQueue MessageQueue { get; set; }

        public DispatcherTimer Timer { get; set; }
        public Stopwatch Stopwatch { get; set; }

        public string Errors
        {
            get => _errors;
            set
            {
                _errors = value;
                OnPropertyChanged();
            }
        }

        public string TimeSpent
        {
            get => _timeSpent;
            set
            {
                _timeSpent = value;
                OnPropertyChanged();
            }
        }

        public ICommand PreviewDropCommand
        {
            get => _previewDropCommand ?? (_previewDropCommand = new DelegateCommand(HandlePreviewDrop));
            set
            {
                _previewDropCommand = value;
                OnPropertyChanged();
            }
        }

        public bool IgnoreSelectedText
        {
            get => _ignoreSelectedText;
            set
            {
                _ignoreSelectedText = value;
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

        public string FileName
        {
            get => _fileName;
            set
            {
                _fileName = value;
                OnPropertyChanged();
            }
        }

        public int NumberOfFiles
        {
            get => _numberOfFiles;
            set
            {
                _numberOfFiles = value;
                OnPropertyChanged();
            }
        }

        public int NumberOfTransliteratedFiles
        {
            get => _numberOfTransliteratedFiles;
            set
            {
                _numberOfTransliteratedFiles = value;
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

        public int PercentOfSymbols
        {
            get => _percentOfSymbols;
            set
            {
                _percentOfSymbols = value;
                OnPropertyChanged();
            }
        }

        public string Left
        {
            get => _left;
            set
            {
                _left = value;
                OnPropertyChanged();
            }
        }

        public string WordProgress
        {
            get => _wordProgress;
            set
            {
                _wordProgress = value;
                OnPropertyChanged();
            }
        }

        public string SymbolProgress
        {
            get => _symbolProgress;
            set
            {
                _symbolProgress = value;
                OnPropertyChanged();
            }
        }

        public ICommand SelectFile
        {
            get
            {
                return new DelegateCommand(o =>
                {
                    // Создаем экземпляр диалогового окна выбора файла
                    var dlg = new OpenFileDialog
                    {
                        DefaultExt = ".*",
                        Filter =
                            "Файлы (.doc; .docx; .xls; .xlsx; .ppt; .pptx; .txt; .pdf; .rtf)|*.doc;*.docx; *.xls;*.xlsx; *.ppt; *.pptx; *.txt; *.pdf; *.rtf"
                    };

                    // Открываем диалоговое окно
                    var result = dlg.ShowDialog();

                    if (result != true) return;

                    if (!Model.CollectionExists())
                    {
                        MessageQueue.Enqueue(GetRes("SnackBarDatabaseNotFound"));
                        return;
                    }

                    Task.Factory.StartNew(() =>
                    {
                        if (Model.TransliterationState)
                        {
                            Model.AddFiles(new[] {dlg.FileName});
                            MessageQueue.Enqueue($"{GetRes("SnackBarAdded")} 1");
                            return;
                        }

                        ProgressBarVisibility = Visibility.Visible;

                        Timer.Start();
                        Stopwatch.Reset();
                        Stopwatch.Start();

                        var success = Model.TranslitFiles(new[] {dlg.FileName}, IgnoreSelectedText);

                        Stopwatch.Stop();
                        Timer.Stop();

                        ProgressBarVisibility = Visibility.Hidden;

                        if (success)
                        {
                            MessageQueue.Enqueue($"{GetRes("SnackBarTransliterationCompleted")} {Model.Errors}");
                            if (!Settings.Default.SoundNotification) return;
                            var soundPlayer = new SoundPlayer(@"sounds\notification.wav");
                            soundPlayer.PlaySync();
                        }
                        else
                        {
                            MessageQueue.Enqueue(GetRes("SnackBarStoppedByUser"));
                        }

                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                    });
                });
            }
        }

        public ICommand SelectFolder
        {
            get
            {
                return new DelegateCommand(o =>
                {
                    using (var fbd = new FolderBrowserDialog())
                    {
                        // Открываем диалоговое окно
                        var result = fbd.ShowDialog();

                        if (result != DialogResult.OK || string.IsNullOrWhiteSpace(fbd.SelectedPath)) return;

                        if (!Model.CollectionExists())
                        {
                            MessageQueue.Enqueue(GetRes("SnackBarDatabaseNotFound"));
                            return;
                        }

                        string[] extensions =
                        {
                            ".doc",
                            ".docx",
                            ".xls",
                            ".xlsx",
                            ".ppt",
                            ".pptx",
                            ".txt",
                            ".pdf",
                            ".rtf"
                        };

                        // Получаем пути всех нескрытых файлов с расширением из массива extensions
                        var files = new DirectoryInfo(fbd.SelectedPath).EnumerateFiles()
                            .Where(f => (f.Attributes & FileAttributes.Hidden) == 0 &&
                                        extensions.Contains(f.Extension.ToLower()))
                            .Select(f => f.FullName).ToArray();

                        if (files.Length <= 0) return;

                        Task.Factory.StartNew(() =>
                        {
                            if (Model.TransliterationState)
                            {
                                Model.AddFiles(files);
                                MessageQueue.Enqueue($"{GetRes("SnackBarAdded")} {files.Length}");
                                return;
                            }

                            ProgressBarVisibility = Visibility.Visible;

                            Timer.Start();
                            Stopwatch.Reset();
                            Stopwatch.Start();

                            var success = Model.TranslitFiles(files, IgnoreSelectedText);

                            Stopwatch.Stop();
                            Timer.Stop();

                            ProgressBarVisibility = Visibility.Hidden;

                            if (success)
                            {
                                MessageQueue.Enqueue($"{GetRes("SnackBarTransliterationCompleted")} {Model.Errors}");
                                if (!Settings.Default.SoundNotification) return;
                                var soundPlayer = new SoundPlayer(@"sounds\notification.wav");
                                soundPlayer.PlaySync();
                            }
                            else
                            {
                                MessageQueue.Enqueue(GetRes("SnackBarStoppedByUser"));
                            }

                            GC.Collect();
                            GC.WaitForPendingFinalizers();
                        });
                    }
                });
            }
        }

        public ICommand Stop
        {
            get
            {
                return new DelegateCommand(o =>
                {
                    if (!Model.Stop)
                        Model.StopTransliteration();
                    else
                        MessageQueue.Enqueue(GetRes("SnackBarPleaseWait"));
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

        private void HandlePreviewDrop(object inObject)
        {
            if (!Model.CollectionExists())
            {
                MessageQueue.Enqueue(GetRes("SnackBarDatabaseNotFound"));
                return;
            }

            if (!(inObject is IDataObject ido)) return;

            if (!(ido.GetData(DataFormats.FileDrop, true) is string[] transferredFiles)) return;

            string[] extensions =
            {
                ".doc",
                ".docx",
                ".xls",
                ".xlsx",
                ".ppt",
                ".pptx",
                ".txt",
                ".pdf",
                ".rtf"
            };

            var filteredFiles = transferredFiles.Where(tf => extensions.Contains(new FileInfo(tf).Extension.ToLower()))
                .ToArray();

            if (filteredFiles.Length > 0)
            {
                Task.Factory.StartNew(() =>
                {
                    if (Model.TransliterationState)
                    {
                        Model.AddFiles(filteredFiles);
                        MessageQueue.Enqueue($"{GetRes("SnackBarAdded")} {filteredFiles.Length}");
                        return;
                    }

                    ProgressBarVisibility = Visibility.Visible;

                    Timer.Start();
                    Stopwatch.Reset();
                    Stopwatch.Start();

                    var success = Model.TranslitFiles(filteredFiles, IgnoreSelectedText);

                    Stopwatch.Stop();
                    Timer.Stop();

                    ProgressBarVisibility = Visibility.Hidden;

                    if (success)
                    {
                        MessageQueue.Enqueue($"{GetRes("SnackBarTransliterationCompleted")} {Model.Errors}");
                        if (!Settings.Default.SoundNotification) return;
                        var soundPlayer = new SoundPlayer(@"sounds\notification.wav");
                        soundPlayer.PlaySync();
                    }
                    else
                    {
                        MessageQueue.Enqueue(GetRes("SnackBarStoppedByUser"));
                    }

                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                });
            }
            else if (!transferredFiles[0].Contains('.'))
            {
                // Получаем пути всех нескрытых файлов с расширением из массива extensions
                var files = new DirectoryInfo(transferredFiles[0]).EnumerateFiles()
                    .Where(f => (f.Attributes & FileAttributes.Hidden) == 0 &&
                                extensions.Contains(f.Extension.ToLower()))
                    .Select(f => f.FullName).ToArray();

                if (files.Length <= 0) return;

                Task.Factory.StartNew(() =>
                {
                    if (Model.TransliterationState)
                    {
                        Model.AddFiles(files);
                        MessageQueue.Enqueue($"{GetRes("SnackBarAdded")} {files.Length}");
                        return;
                    }

                    ProgressBarVisibility = Visibility.Visible;

                    Timer.Start();
                    Stopwatch.Reset();
                    Stopwatch.Start();

                    var success = Model.TranslitFiles(files, IgnoreSelectedText);

                    Stopwatch.Stop();
                    Timer.Stop();

                    ProgressBarVisibility = Visibility.Hidden;

                    if (success)
                    {
                        MessageQueue.Enqueue($"{GetRes("SnackBarTransliterationCompleted")} {Model.Errors}");
                        if (!Settings.Default.SoundNotification) return;
                        var soundPlayer = new SoundPlayer(@"sounds\notification.wav");
                        soundPlayer.PlaySync();
                    }
                    else
                    {
                        MessageQueue.Enqueue(GetRes("SnackBarStoppedByUser"));
                    }

                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                });
            }
        }

        private void UpdateTimeSpent(object sender, EventArgs e)
        {
            var span = Stopwatch.Elapsed;
            TimeSpent = $"{span.Hours:00}:{span.Minutes:00}:{span.Seconds:00}";
        }

        private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            FileName = CutText(Model.FileName, 75);
            PercentOfWords = Model.PercentOfWords;
            PercentOfSymbols = Model.PercentOfSymbols;

            Left = $"{GetRes("TextBlockLeft")}: {Model.Left}";
            WordProgress = $"{GetRes("TextBlockTransliterationOfExceptionWords")}: {PercentOfWords}%";
            SymbolProgress = $"{GetRes("TextBlockCharacterTransliteration")}: {PercentOfSymbols}%";
        }

        private string CutText(string text, int requiredLength)
        {
            if (text == null) return "";
            if (text.Length <= requiredLength) return text;

            var left = text.Substring(0, requiredLength / 2 - 3);
            var right = text.Substring(text.Length - requiredLength / 2);
            return $"{left}...{right}";
        }
    }
}