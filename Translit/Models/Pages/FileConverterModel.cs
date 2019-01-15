using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using LiteDB;
using Microsoft.Office.Interop.Excel;
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
		private int _percentOfWords;
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

		public int PercentOfWords
		{
			get => _percentOfWords;
			set
			{
				if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
				_percentOfWords = value;
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

		public void TranslitFiles(string[] files, bool? ignoreSelectedText)
		{
			NumberOfDocuments = files.Length;
			
			// Перебор и транслитерация всех полученных файлов
			for (var i = 0; i < NumberOfDocuments; i++)
			{
				NumberOfDocumentsTranslated = i;
				if (files[i].ToLower().EndsWith(".doc") || files[i].ToLower().EndsWith(".docx"))
				{
					TranslitWordFile(files[i], ignoreSelectedText);
				}
				else if (files[i].ToLower().EndsWith(".xls") || files[i].ToLower().EndsWith(".xlsx"))
				{
					TranslitExelFile(files[i]);
				}
			}
		}

		// Транслитерация файла Word
		private void TranslitWordFile(string filename, bool? ignoreSelectedText)
		{
			var isConverted = false;

			if (filename.ToLower().EndsWith(".doc"))
			{
				filename = ConvertDocToDocx(filename);
				isConverted = true;
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

						if (ignoreSelectedText != null && ignoreSelectedText == true)
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
					PercentOfWords = i * 100 / (words.Length - 1);
				}

				var symbols = db.GetCollection<Symbol>("Symbols").FindAll().ToArray();

				for (var i = 0; i < symbols.Length; i++)
				{
					IEnumerable<XElement> nodes;

					if (ignoreSelectedText != null && ignoreSelectedText == true)
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

			CreateNewFileFromDirectory(temporaryFolder, filename, isConverted);
		}

		// Транслитерация файла Exel
		private void TranslitExelFile(string filename)
		{
			var isConverted = false;

			if (filename.ToLower().EndsWith(".xls"))
			{
				filename = ConvertXlsToXlsx(filename);
				isConverted = true;
			}

			// Распаковка документа и получение расположения распакованной папки
			var temporaryFolder = UnZipFileToTemporaryFolder(filename);

			// Определяем путь к xml файлу распакованного документа
			var xmlPath = temporaryFolder + @"\xl\sharedStrings.xml";

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

						var nodes = doc.Descendants().Where(n => n.Name.LocalName == "t");

						// Перебор и замена
						foreach (var n in nodes)
						{
							n.Value = n.Value.Replace(cyryllic, latin);
						}
					}

					// Высчитываем процент текущего прогресса
					PercentOfWords = i * 100 / (words.Length - 1);
				}

				var symbols = db.GetCollection<Symbol>("Symbols").FindAll().ToArray();

				for (var i = 0; i < symbols.Length; i++)
				{
					var nodes = doc.Descendants().Where(n => n.Name.LocalName == "t");

					// Перебор и замена
					foreach (var n in nodes)
					{
						n.Value = n.Value.Replace(symbols[i].Cyryllic, symbols[i].Latin);
					}

					PercentOfSymbols = i * 100 / (symbols.Length - 1);
				}
			}

			doc.Save(xmlPath);

			CreateNewFileFromDirectory(temporaryFolder, filename, isConverted);
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

		private void CreateNewFileFromDirectory(string folder, string filename, bool isConverted)
		{
			var fileInfo = new FileInfo(filename);

			// Получение данных о новом файле
			var directoryName = fileInfo.DirectoryName;
			var fileName = fileInfo.Name.Substring(0, fileInfo.Name.Length - fileInfo.Extension.Length);
			var label = $" ({GetRes("FileNameLatin")})";
			var extension = fileInfo.Extension;

			// Комбинирование данных
			var newFileName = $@"{directoryName}\{fileName}{label}{extension}";

			Debug.WriteLine(newFileName);

			if (File.Exists(newFileName))
			{
				File.Delete(newFileName);
			}

			ZipFile.CreateFromDirectory(folder, newFileName);

			// Удаление временной папки
			Directory.Delete(folder, true);

			// Если автосохранение отключено и файл не был конвертирован в новый формат
			if (Settings.Default.AutoSave == false && isConverted == false)
			{
				try
				{
					// Замена файла
					File.Replace(newFileName, filename, null);
				}
				catch (Exception)
				{
					// ignored
				}
			}

			// Если файл был конвертирован в новый формат, удаляем не отмеченный файл
			if (isConverted)
			{
				File.Delete(filename);
			}
		}

		public string ConvertDocToDocx(string filename)
		{
			var word = new Microsoft.Office.Interop.Word.Application();
			var document = word.Documents.Open(filename);

			var newFileName = filename + "x";

			document.SaveAs2(newFileName, WdSaveFormat.wdFormatXMLDocument, CompatibilityMode: WdCompatibilityMode.wdWord2010);
			word.ActiveDocument.Close();
			word.Quit();

			if (!Settings.Default.AutoSave)
			{
				File.Delete(filename);
			}

			return newFileName;
		}

		private string ConvertXlsToXlsx(string filename)
		{
			var exel = new Microsoft.Office.Interop.Excel.Application();
			var workbook = exel.Workbooks.Open(filename);

			var newFileName = filename + "x";

			workbook.SaveAs(newFileName, XlFileFormat.xlOpenXMLWorkbook);
			workbook.Close();
			exel.Quit();

			if (!Settings.Default.AutoSave)
			{
				File.Delete(filename);
			}

			return newFileName;
		}

		public string GetRes(string key)
		{
			return Application.Current.Resources[key].ToString();
		}
	}
}
