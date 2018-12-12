using LiteDB;
using System;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using System.Xml.Linq;
using MaterialDesignThemes.Wpf;
using Translit.Entity;
using Translit.Properties;
using Application = System.Windows.Application;

namespace Translit.Pages
{
	public partial class FileConverterPage
	{
		public Snackbar SnackbarInfo { get; set; }
		public string ConnectionString { get; set; }

		public FileConverterPage(Snackbar snackbar)
		{
			InitializeComponent();
			SnackbarInfo = snackbar;
			ConnectionString = ConfigurationManager.ConnectionStrings["LiteDatabaseConnection"].ConnectionString;
		}

		// Асинхронный показ уведомления
		private async Task ShowAsyncNotification(string resourceName)
		{
			await Task.Factory.StartNew(() => { })
				.ContinueWith(t => { SnackbarInfo.MessageQueue.Enqueue(GetRes(resourceName)); },
					TaskScheduler.FromCurrentSynchronizationContext());
		}

		// Нажатие кнопки выбора файла Word
		private async void ButtonSelectFile_Click(object sender, RoutedEventArgs e)
		{
			// Создаем экземпляр диалогового окна выбора файла
			var dlg = new Microsoft.Win32.OpenFileDialog()
			{
				FileName = "Document",
				DefaultExt = ".*",
				Filter = "Text documents (.doc; .docx)|*.doc;*.docx" // Фильтрация файлов
			};

			// Открываем диалоговое окно
			var result = dlg.ShowDialog();
			if (result != true) return;
			ButtonSelectFile.IsEnabled = false;
			ButtonSelectFolder.IsEnabled = false;

			// Раскрываем прогресс
			StackPanelProgress.Visibility = Visibility.Visible;
			ProgressBarDocuments.Value = 1;
			ProgressBarDocuments.Maximum = 1;
			TextBlockDocumetns.Text = GetRes("TextBlockDocuments") + ": 1/1";
			ProgressBarExceptions.Value = 0;
			ProgressBarDocument.Value = 0;

			// Транслитерация выбранного файла
			await Task.Factory.StartNew(() => TranslitFile(dlg.FileName));

			// Скрываем прогресс
			StackPanelProgress.Visibility = Visibility.Hidden;

			// Выводим уведомление
			await ShowAsyncNotification("TransliterationCompleted");
			ButtonSelectFile.IsEnabled = true;
			ButtonSelectFolder.IsEnabled = true;
		}

		// Нажатие кнопки выбора папки с документами
		private async void ButtonSelectFolder_Click(object sender, RoutedEventArgs e)
		{
			using (var fbd = new FolderBrowserDialog())
			{
				// Открываем диалоговое окно
				var result = fbd.ShowDialog();
				if (result != DialogResult.OK || string.IsNullOrWhiteSpace(fbd.SelectedPath)) return;
				ButtonSelectFile.IsEnabled = false;
				ButtonSelectFolder.IsEnabled = false;

				// Получаем пути всех нескрытых файлов с расширением .doc и .docx
				var files = new DirectoryInfo(fbd.SelectedPath).EnumerateFiles()
					.Where(f => (f.Attributes & FileAttributes.Hidden) == 0 && (f.Extension == ".doc" || f.Extension == ".docx"))
					.Select(f => f.FullName).ToArray();

				// Раскрываем прогресс
				StackPanelProgress.Visibility = Visibility.Visible;
				ProgressBarDocuments.Value = 0;
				ProgressBarDocuments.Maximum = files.Length - 1;

				// Перебор и транслитерация всех полученных файлов
				for (int[] i = { 0 }; i[0] < files.Length; i[0]++)
				{
					TextBlockDocumetns.Dispatcher.Invoke(() =>
					{
						var text = GetRes("TextBlockDocuments") + ": " + (i[0] + 1) + "/" + files.Length;
						TextBlockDocumetns.Text = text;
					}, DispatcherPriority.Background);
					ProgressBarDocuments.Value = i[0];
					ProgressBarExceptions.Value = 0;
					ProgressBarDocument.Value = 0;
					await Task.Factory.StartNew(() => TranslitFile(files[i[0]]));
				}

				StackPanelProgress.Visibility = Visibility.Hidden;
				await ShowAsyncNotification("TransliterationCompleted");
				ButtonSelectFile.IsEnabled = true;
				ButtonSelectFolder.IsEnabled = true;
			}
		}

		// Транслитерация файла
		private void TranslitFile(string filename)
		{
			// Получение информации о файле
			var fileInfo = new FileInfo(filename);

			// Путь временной папки
			var temporaryFolder = fileInfo.DirectoryName + @"\" + DateTime.Now.ToFileTime();

			// Распаковка документа во временную папку
			using (var archive = ZipFile.OpenRead(filename))
			{
				archive.ExtractToDirectory(temporaryFolder);
			}

			// Определяем путь к xml файлу распакованного документа
			var xmlPath = temporaryFolder + @"\word\document.xml";

			// Открваем документ xml
			var doc = XDocument.Load(xmlPath);
			using (var db = new LiteDatabase(ConnectionString))
			{
				var words = db.GetCollection<Word>("Words").FindAll().ToArray();
				for (var i = 0; i < words.Length; i++)
				{
					// Трансформируем слово в три различных состояния [ЗАГЛАВНЫЕ, прописные и Первыя заглавная] и заменяем
					for (var j = 0; j < 3; j++)
					{
						var cyryllic = words[i].Cyryllic;
						var latin = words[i].Latin;
						switch (j)
						{
							case 0:
								{
									cyryllic = cyryllic.ToUpper();
									latin = latin.ToUpper();
									break;
								}
							case 1:
								{
									cyryllic = cyryllic.ToLower();
									latin = latin.ToLower();
									break;
								}
							case 2:
								{
									cyryllic = cyryllic.First().ToString().ToUpper() + cyryllic.Substring(1);
									latin = latin.First().ToString().ToUpper() + latin.Substring(1);
									break;
								}
						}

						// Получение всех узлов <w:t>
						var elements = doc.Descendants().Where(x => x.Name.LocalName == "t");

						// Перебор и замена
						foreach (var e in elements)
						{
							e.Value = e.Value.Replace(cyryllic, latin);
						}
					}

					// Высчитываем процент текущего прогресса
					long percent = i * 100 / (words.Length - 1);

					// Выводим результат
					TextBlockExceptions.Dispatcher.Invoke(
						() => { TextBlockExceptions.Text = GetRes("TextBlockTransliterationOfExceptionWords") + ": " + percent + "%"; },
						DispatcherPriority.Background);
					// Изменяем прогресс
					ProgressBarExceptions.Dispatcher.Invoke(() => ProgressBarExceptions.Value = percent,
						DispatcherPriority.Background);
				}

				var symbols = db.GetCollection<Symbol>("Symbols").FindAll().ToArray();
				for (var i = 0; i < symbols.Length; i++)
				{
					var elements = doc.Descendants().Where(x => x.Name.LocalName == "t");

					// Перебор и замена
					foreach (var e in elements)
					{
						e.Value = e.Value.Replace(symbols[i].Cyryllic, symbols[i].Latin);
					}

					long percent = i * 100 / (symbols.Length - 1);
					TextBlockDocument.Dispatcher.Invoke(
						() => TextBlockDocument.Text = GetRes("TextBlockCharacterTransliteration") + ": " + percent + "%",
						DispatcherPriority.Background);
					ProgressBarDocument.Dispatcher.Invoke(() => ProgressBarDocument.Value = percent, DispatcherPriority.Background);
				}
			}

			doc.Save(xmlPath);
			int number = 1;

			// Создание документа
			while (true)
			{
				// Получение данных о новом файле
				var directoryName = fileInfo.DirectoryName;
				var fileName = fileInfo.Name.Substring(0, fileInfo.Name.Length - fileInfo.Extension.Length);
				string label;
				if (number == 1)
				{
					label = " (" + GetRes("FileNameLatin") + ")";
				}
				else
				{
					label = " (" + GetRes("FileNameLatin") + " " + number + ")";
				}

				var extension = fileInfo.Extension;

				// Комбинирование данных
				var newDocument = directoryName + @"\" + fileName + label + extension;
				if (File.Exists(newDocument))
				{
					number++;
					continue;
				}

				ZipFile.CreateFromDirectory(temporaryFolder, newDocument);

				// Удаление временной папки
				Directory.Delete(temporaryFolder, true);
				if (Settings.Default.AutoSave) break;
				try
				{
					// Замена файла
					File.Replace(newDocument, filename, null);
				}
				catch (Exception)
				{
					// ignored
				}

				break;
			}
		}

		private string GetRes(string key)
		{
			return Application.Current.Resources[key].ToString();
		}
	}
}