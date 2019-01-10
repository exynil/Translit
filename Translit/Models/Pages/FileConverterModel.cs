using LiteDB;
using System;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Xml.Linq;
using Microsoft.Office.Interop.Word;
using Translit.Entity;
using Translit.Properties;
using Application = System.Windows.Application;

namespace Translit.Models.Pages
{
	public class FileConverterModel : IFileConverterModel
	{
		public string ConnectionString { get; }

		private int _numberOfDocumentsTranslated;
		private int _numberOfDocuments;
		private int _percentOfExceptions;
		private int _percentOfSymbols;

		public int NumberOfDocumentsTranslated
		{
			get => _numberOfDocumentsTranslated;
			set
			{
				_numberOfDocumentsTranslated = value;
				OnPropertyChanged();
			}
		}
		public int NumberOfDocuments
		{
			get => _numberOfDocuments;
			set
			{
				_numberOfDocuments = value;
				OnPropertyChanged();
			}
		}

		public int PercentOfExceptions
		{
			get => _percentOfExceptions;
			set
			{
				if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
				_percentOfExceptions = value;
				OnPropertyChanged();
			}
		}

		public int PercentOfSymbols
		{
			get => _percentOfSymbols;
			set
			{
				if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
				_percentOfSymbols = value;
				OnPropertyChanged();
			}
		}

		public FileConverterModel()
		{
			ConnectionString = ConfigurationManager.ConnectionStrings["LiteDatabaseConnection"].ConnectionString;
		}

		public event PropertyChangedEventHandler PropertyChanged;
		public void OnPropertyChanged([CallerMemberName]string prop = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
		}

		public string SelectFile()
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
			if (result == true)
			{
				return dlg.FileName;
			}

			return "";
		}

		public string[] SelectFolder()
		{
			using (var fbd = new FolderBrowserDialog())
			{
				// Открываем диалоговое окно
				var result = fbd.ShowDialog();
				if (result != DialogResult.OK || string.IsNullOrWhiteSpace(fbd.SelectedPath)) return null;

				// Получаем пути всех нескрытых файлов с расширением .doc и .docx
				var files = new DirectoryInfo(fbd.SelectedPath).EnumerateFiles()
					.Where(f => (f.Attributes & FileAttributes.Hidden) == 0 && (f.Extension == ".doc" || f.Extension == ".docx"))
					.Select(f => f.FullName).ToArray();
				return files;
			}
		}

		public void TranslitFiles(string[] files)
		{
			NumberOfDocuments = files.Length;
			
			// Перебор и транслитерация всех полученных файлов
			for (int i = 0; i < NumberOfDocuments; i++)
			{
				NumberOfDocumentsTranslated = i;
				TranslitFile(files[i]);
			}
		}

		// Транслитерация файла
		public void TranslitFile(string filename)
		{
			var overwrite = false;
			if (filename.ToLower().EndsWith(".doc"))
			{
				filename = ConvertDocToDocx(filename);
				overwrite = true;
			}

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
								cyryllic = cyryllic.ToUpper();
								latin = latin.ToUpper();
								break;
							case 1:
								cyryllic = cyryllic.ToLower();
								latin = latin.ToLower();
								break;
							case 2:
								cyryllic = cyryllic.First().ToString().ToUpper() + cyryllic.Substring(1);
								latin = latin.First().ToString().ToUpper() + latin.Substring(1);
								break;
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
					PercentOfExceptions = i * 100 / (words.Length - 1);
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

					PercentOfSymbols = i * 100 / (symbols.Length - 1);
				}
			}

			doc.Save(xmlPath);

			BuildDocumentFromFolder(temporaryFolder, filename, overwrite);
		}

		public void BuildDocumentFromFolder(string folder, string filename, bool overwrite)
		{
			var fileInfo = new FileInfo(filename);
			int n = 1;

			// Создание документа
			while (true)
			{
				// Получение данных о новом файле
				var directoryName = fileInfo.DirectoryName;
				var fileName = fileInfo.Name.Substring(0, fileInfo.Name.Length - fileInfo.Extension.Length);
				string label;
				if (n == 1)
				{
					label = " (" + GetRes("FileNameLatin") + ")";
				}
				else
				{
					label = " (" + GetRes("FileNameLatin") + " " + n + ")";
				}

				var extension = fileInfo.Extension;

				// Комбинирование данных
				var newDocument = directoryName + @"\" + fileName + label + extension;
				if (File.Exists(newDocument))
				{
					n++;
					continue;
				}

				ZipFile.CreateFromDirectory(folder, newDocument);

				// Удаление временной папки
				Directory.Delete(folder, true);

				if (Settings.Default.AutoSave && overwrite == false) break;

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

		public string ConvertDocToDocx(string path)
		{
			var word = new Microsoft.Office.Interop.Word.Application();
			var sourceFile = new FileInfo(path);
			var document = word.Documents.Open(sourceFile.FullName);

			// Получение данных о новом файле
			var directoryName = sourceFile.DirectoryName;
			var fileName = sourceFile.Name.Substring(0, sourceFile.Name.Length - sourceFile.Extension.Length);
			var label = " (" + GetRes("FileNameLatin") + ")";

			var extension = ".docx";

			// Комбинирование данных
			var newDocument = directoryName + @"\" + fileName + label + extension;

			document.SaveAs2(newDocument, WdSaveFormat.wdFormatXMLDocument, CompatibilityMode: WdCompatibilityMode.wdWord2010);
			word.ActiveDocument.Close();
			word.Quit();
			if (!Settings.Default.AutoSave)
			{
				File.Delete(path);
			}
			return newDocument;
		}

		public string GetRes(string key)
		{
			return Application.Current.Resources[key].ToString();
		}
	}
}
