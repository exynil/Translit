using LiteDB;
using System;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Threading;
using System.Xml.Linq;
using Translit.Entity;
using Translit.Properties;

namespace Translit.Pages
{
	public partial class FileConverterPage
	{
		public FileConverterPage()
		{
			InitializeComponent();
		}

		// Нажатие кнопки выбора файла Word
		private void ButtonSelectFile_Click(object sender, RoutedEventArgs e)
		{
			// Создаем экземпляр диалогового окна выбора файла
			Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog()
			{
				FileName = "Document",
				DefaultExt = ".*",
				Filter = "Text documents (.doc; .docx)|*.doc;*.docx" // Фильтрация файлов
			};

			// Открываем диалоговое окно
			bool? result = dlg.ShowDialog();
			if (result == true)
			{
				ButtonSelectFile.IsEnabled = false;
				ButtonSelectFolder.IsEnabled = false;
				TextBlockInfo.Visibility = Visibility.Hidden;
				// Транслитерация выбранного файла
				TranslitFile(dlg.FileName);
				// Скрываем прогресс и показываем информацию
				ProgressBarExceptions.Visibility = Visibility.Hidden;
				TextBlockExceptions.Visibility = Visibility.Hidden;
				ProgressBarDocument.Visibility = Visibility.Hidden;
				TextBlockDocument.Visibility = Visibility.Hidden;
				TextBlockInfo.Visibility = Visibility.Visible;
				TextBlockInfo.SetResourceReference(TextBlock.TextProperty, "TransliterationCompleted");
				ButtonSelectFile.IsEnabled = true;
				ButtonSelectFolder.IsEnabled = true;
			}
		}

		// Нажатие кнопки выбора папки с документами
		private void ButtonSelectFolder_Click(object sender, RoutedEventArgs e)
		{
			using (var fbd = new FolderBrowserDialog())
			{
				// Открываем диалоговое окно
				DialogResult result = fbd.ShowDialog();
				if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
				{
					// Получаем все нескрытые файлы и с расширением .doc и .docx
					var noHiddenDocFiles = new DirectoryInfo(fbd.SelectedPath).EnumerateFiles()
						.Where(f => (f.Attributes & FileAttributes.Hidden) == 0 && (f.Extension == ".doc" || f.Extension == ".docx"))
						.ToArray();

					// Извлекаем полный путь всех файлов
					string[] files = noHiddenDocFiles.Select(f => f.FullName).ToArray();

					// Манипуляции с индикатором прогресса
					ProgressBarDocuments.Value = 0;
					ProgressBarDocuments.Maximum = files.Length - 1;
					ProgressBarDocuments.Visibility = Visibility.Visible;
					TextBlockInfo.Visibility = Visibility.Hidden;

					// Перебор и транслитерация всех файлов
					for (int[] i = {0}; i[0] < files.Length; i[0]++)
					{
						TextBlockDocumetns.Text = System.Windows.Application.Current.Resources["TextBlockDocuments"] + ": " + (i[0] + 1) +
						                          "/" + files.Length;
						ProgressBarDocuments.Dispatcher.Invoke(() => ProgressBarDocuments.Value = i[0], DispatcherPriority.Background);
						TranslitFile(files[i[0]]);
					}

					ProgressBarDocuments.Visibility = Visibility.Hidden;
					TextBlockDocumetns.Visibility = Visibility.Hidden;
					ProgressBarExceptions.Visibility = Visibility.Hidden;
					TextBlockExceptions.Visibility = Visibility.Hidden;
					ProgressBarDocument.Visibility = Visibility.Hidden;
					TextBlockDocument.Visibility = Visibility.Hidden;
					TextBlockInfo.Visibility = Visibility.Visible;
					TextBlockInfo.SetResourceReference(TextBlock.TextProperty, "TransliterationCompleted");
				}
			}
		}

		// Транслитерация файла
		private void TranslitFile(string filename)
		{
			// Получение информации о файле
			FileInfo fileInfo = new FileInfo(filename);

			// Путь временной папки
			string temporaryFolder = fileInfo.DirectoryName + @"\" + DateTime.Now.ToFileTime();

			// Распаковка документа во временную папку
			using (ZipArchive archive = ZipFile.OpenRead(filename))
			{
				archive.ExtractToDirectory(temporaryFolder);
			}

			// Определяем путь к xml файлу распакованного документа
			string xmlPath = temporaryFolder + @"\word\document.xml";

			// Открваем документ xml
			XDocument doc = XDocument.Load(xmlPath);
			using (LiteDatabase db =
				new LiteDatabase(ConfigurationManager.ConnectionStrings["LiteDatabaseConnection"].ConnectionString))
			{
				var words = db.GetCollection<Word>("Words").FindAll().ToArray();
				ProgressBarExceptions.Value = 0;
				ProgressBarExceptions.Visibility = Visibility.Visible;
				TextBlockExceptions.Visibility = Visibility.Visible;
				for (int i = 0; i < words.Length; i++)
				{
					// Трансформируем слово в три различных состояния [ЗАГЛАВНЫЕ, прописные и Первыя заглавная] и заменяем
					for (int j = 0; j < 3; j++)
					{
						string cyryllic = words[i].Cyryllic;
						string latin = words[i].Latin;
						if (j == 0)
						{
							cyryllic = cyryllic.ToUpper();
							latin = latin.ToUpper();
						}
						else if (j == 1)
						{
							cyryllic = cyryllic.ToLower();
							latin = latin.ToLower();
						}
						else if (j == 2)
						{
							cyryllic = cyryllic.First().ToString().ToUpper() + cyryllic.Substring(1);
							latin = latin.First().ToString().ToUpper() + latin.Substring(1);
						}

						// Получение всех узлов <w:t>
						var xElements = doc.Descendants().Where(x => x.Name.LocalName == "t");

						// Перебор и замена
						foreach (XElement xElement in xElements)
						{
							xElement.Value = xElement.Value.Replace(cyryllic, latin);
						}
					}

					// Высчитываем процент текущего прогресса
					long percent = i * 100 / (words.Length - 1);
					// Выводим результат
					TextBlockExceptions.Text =
						System.Windows.Application.Current.Resources["TextBlockTransliterationOfExceptionWords"] + ": " + percent + "%";
					// Изменяем прогресс
					ProgressBarExceptions.Dispatcher.Invoke(() => ProgressBarExceptions.Value = percent,
						DispatcherPriority.Background);
				}

				var symbols = db.GetCollection<Symbol>("Symbols").FindAll().ToArray();
				ProgressBarDocument.Value = 0;
				ProgressBarDocument.Visibility = Visibility.Visible;
				TextBlockDocument.Visibility = Visibility.Visible;
				for (int i = 0; i < symbols.Length; i++)
				{
					var xElements = doc.Descendants().Where(x => x.Name.LocalName == "t");

					// Перебор и замена
					foreach (XElement xElement in xElements)
					{
						xElement.Value = xElement.Value.Replace(symbols[i].Cyryllic, symbols[i].Latin);
					}

					long percent = i * 100 / (symbols.Length - 1);
					TextBlockDocument.Text = System.Windows.Application.Current.Resources["TextBlockCharacterTransliteration"] + ": " +
					                         percent + "%";
					ProgressBarDocument.Dispatcher.Invoke(() => ProgressBarDocument.Value = percent, DispatcherPriority.Background);
				}
			}

			doc.Save(xmlPath);
			string newDocumentFullName = fileInfo.FullName.Substring(0, fileInfo.FullName.Length - fileInfo.Extension.Length) +
			                             " (" + System.Windows.Application.Current.Resources["FileNameLatin"] + ")" +
			                             fileInfo.Extension;
			ZipFile.CreateFromDirectory(temporaryFolder, newDocumentFullName);
			Directory.Delete(temporaryFolder, true);
			if (!Settings.Default.AutoSave)
			{
				try
				{
					File.Delete(filename);
				}
				catch (Exception)
				{
					// ignored
				}
			}
		}
	}
}