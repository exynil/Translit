using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using LiteDB;
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

		public void TranslitFiles(string[] files, bool? ignoreMarkers)
		{
			NumberOfDocuments = files.Length;
			
			// Перебор и транслитерация всех полученных файлов
			for (var i = 0; i < NumberOfDocuments; i++)
			{
				NumberOfDocumentsTranslated = i;
				TranslitFile(files[i], ignoreMarkers);
			}
		}

		// Транслитерация файла
		public void TranslitFile(string filename, bool? ignoreMarkers)
		{
			var overwrite = false;

			if (filename.ToLower().EndsWith(".doc"))
			{
				filename = ConvertDocToDocx(filename);
				overwrite = true;
			}

			// Распаковка документа и получение расположения распакованной папки
			var temporaryFolder = UnZipFileToTemporaryFolder(filename);

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

						IEnumerable<XElement> nodes;

						if (ignoreMarkers != null && ignoreMarkers == true)
						{
							nodes = doc.Descendants()
								.Where(n => n.Name.LocalName == "r" &&
								            n.Descendants()
									            .Where(r => r.Name.LocalName == "highlight")
									            .ToList()
									            .Count ==
								            0)
								.Where(n => n.Name.LocalName == "t");
						}
						else
						{
							nodes = doc.Descendants().Where(n => n.Name.LocalName == "t");
						}
						

						// Перебор и замена
						foreach (var n in nodes)
						{
							n.Value = n.Value.Replace(cyryllic, latin);
						}
					}

					// Высчитываем процент текущего прогресса
					PercentOfExceptions = i * 100 / (words.Length - 1);
				}

				var symbols = db.GetCollection<Symbol>("Symbols").FindAll().ToArray();

				for (var i = 0; i < symbols.Length; i++)
				{
					IEnumerable<XElement> nodes;

					if (ignoreMarkers != null && ignoreMarkers == true)
					{
						var rNodes = doc.Descendants().Where(n => n.Name.LocalName == "r" && n.Descendants().Where(r => r.Name.LocalName == "highlight").ToList().Count == 0);

						nodes = rNodes.Descendants().Where(n => n.Name.LocalName == "t");
					}
					else
					{
						nodes = doc.Descendants().Where(n => n.Name.LocalName == "t");
					}

					// Перебор и замена
					foreach (var n in nodes)
					{
						n.Value = n.Value.Replace(symbols[i].Cyryllic, symbols[i].Latin);
					}

					PercentOfSymbols = i * 100 / (symbols.Length - 1);
				}
			}

			doc.Save(xmlPath);

			BuildDocumentFromFolder(temporaryFolder, filename, overwrite);
		}

		private static string UnZipFileToTemporaryFolder(string filename)
		{
			// Получение информации о файле
			var fileInfo = new FileInfo(filename);

			// Путь временной папки
			var temporaryFolder = $@"{fileInfo.DirectoryName}\${DateTime.Now.ToFileTime()}";

			// Распаковка документа во временную папку
			using (var archive = ZipFile.OpenRead(filename))
			{
				archive.ExtractToDirectory(temporaryFolder);
			}

			return temporaryFolder;
		}

		private void BuildDocumentFromFolder(string folder, string filename, bool overwrite)
		{
			var fileInfo = new FileInfo(filename);

			// Получение данных о новом файле
			var directoryName = fileInfo.DirectoryName;
			var fileName = fileInfo.Name.Substring(0, fileInfo.Name.Length - fileInfo.Extension.Length);
			var label = $" ({GetRes("FileNameLatin")})";
			var extension = fileInfo.Extension;

			// Комбинирование данных
			var newDocument = $@"{directoryName}\{fileName}{label}{extension}";

			if (File.Exists(newDocument))
			{
				File.Delete(newDocument);
			}

			ZipFile.CreateFromDirectory(folder, newDocument);

			// Удаление временной папки
			Directory.Delete(folder, true);

			if (Settings.Default.AutoSave && overwrite == false) return;

			try
			{
				// Замена файла
				File.Replace(newDocument, filename, null);
			}
			catch (Exception)
			{
				// ignored
			}
		}

		public string ConvertDocToDocx(string path)
		{
			var word = new Microsoft.Office.Interop.Word.Application();
			var fileInfo = new FileInfo(path);
			var document = word.Documents.Open(fileInfo.FullName);

			// Получение данных о новом файле
			var directoryName = fileInfo.DirectoryName;
			var fileName = fileInfo.Name.Substring(0, fileInfo.Name.Length - fileInfo.Extension.Length);
			var label = $" ({GetRes("FileNameLatin")})";

			var extension = ".docx";

			// Комбинирование данных
			var newDocument = $@"{directoryName}\{fileName}{label}{extension}";

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
