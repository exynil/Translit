using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;
using Translit.Entity;
using Translit.Models.Pages;
using Application = System.Windows.Application;

namespace Translit.ViewModels.Pages
{
	class FileConverterViewModel : INotifyPropertyChanged
	{
		private bool _ignoreSelectedText;
		private bool _canSelectFileAndFolder;
		private Visibility _progressBarVisibility;
		private string _fileName;
		private int _numberOfFiles;
		private int _numberOfTransliteratedFiles;
		private int _percentOfWords;
		private int _percentOfSymbols;
		private string _fileProgress;
		private string _wordProgress;
		private string _symbolProgress;
		public event PropertyChangedEventHandler PropertyChanged;
		public FileConverterModel Model { get; set; }
		public SnackbarMessageQueue MessageQueue { get; set; }

		public bool IgnoreSelectedText
		{
			get => _ignoreSelectedText;
			set
			{
				_ignoreSelectedText = value;
				OnPropertyChanged();
			}
		}

		public bool CanSelectFileAndFolder
		{
			get => _canSelectFileAndFolder;
			set
			{
				_canSelectFileAndFolder = value;
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

		public string FileProgress
		{
			get => _fileProgress;
			set
			{
				_fileProgress = value;
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

		public FileConverterViewModel()
		{
			Model = new FileConverterModel();
			Model.PropertyChanged += OnModelPropertyChanged;
			MessageQueue = new SnackbarMessageQueue();
			CanSelectFileAndFolder = true;
			ProgressBarVisibility = Visibility.Hidden;
		}

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private string GetRes(string key)
		{
			return Application.Current.Resources[key].ToString();
		}

		public ICommand SelectFile
		{
			get
			{
				return new DelegateCommand(o =>
				{

					// Создаем экземпляр диалогового окна выбора файла
					var dlg = new Microsoft.Win32.OpenFileDialog
					{
						DefaultExt = ".*",
						Filter = "Файлы (.doc; .docx; .xls; .xlsx; .ppt; .pptx; .txt; .pdf; .rtf)|*.doc;*.docx; *.xls;*.xlsx; *.ppt; *.pptx; *.txt; *.pdf; *.rtf"
					};

					// Открываем диалоговое окно
					var result = dlg.ShowDialog();

					if (result != true) return;

					if (!File.Exists(@"Database\localdb.db"))
					{
						MessageQueue.Enqueue(GetRes("SnackBarDatabaseNotFound"));
						return;
					}

					Task.Factory.StartNew(() =>
					{
						CanSelectFileAndFolder = false;
						ProgressBarVisibility = Visibility.Visible;

						Model.TranslitFiles(new[] { dlg.FileName }, IgnoreSelectedText);

						ProgressBarVisibility = Visibility.Hidden;
						CanSelectFileAndFolder = true;
						MessageQueue.Enqueue(GetRes("SnackBarTransliterationCompleted"));
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

						if (!File.Exists(@"Database\localdb.db"))
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
							.Where(f => (f.Attributes & FileAttributes.Hidden) == 0 && extensions.Contains(f.Extension))
							.Select(f => f.FullName).ToArray();

						if (files.Length <= 0) return;

						Task.Factory.StartNew(() =>
						{
							CanSelectFileAndFolder = false;
							ProgressBarVisibility = Visibility.Visible;

							Model.TranslitFiles(files, IgnoreSelectedText);

							ProgressBarVisibility = Visibility.Hidden;
							CanSelectFileAndFolder = true;
							MessageQueue.Enqueue(GetRes("SnackBarTransliterationCompleted"));
						});
					}
				});
			}
		}

		private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			FileName = Model.FileName;
			NumberOfFiles = Model.NumberOfFiles;
			NumberOfTransliteratedFiles = Model.NumberOfTransliteratedFiles;
			PercentOfWords = Model.PercentOfWords;
			PercentOfSymbols = Model.PercentOfSymbols;

			FileProgress = $"{GetRes("TextBlockFiles")}: {NumberOfTransliteratedFiles}/{NumberOfFiles}";
			WordProgress = $"{GetRes("TextBlockTransliterationOfExceptionWords")}: {PercentOfWords}%";
			SymbolProgress = $"{GetRes("TextBlockCharacterTransliteration")}: {PercentOfSymbols}%";
		}
	}
}
