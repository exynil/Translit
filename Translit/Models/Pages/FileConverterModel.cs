using LiteDB;
using Microsoft.Office.Core;
using Microsoft.Office.Interop.Excel;
using Microsoft.Office.Interop.PowerPoint;
using Microsoft.Office.Interop.Word;
using SautinSoft.Document;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Translit.Entity;
using Translit.Properties;
using Application = System.Windows.Application;
using LoadOptions = System.Xml.Linq.LoadOptions;

namespace Translit.Models.Pages
{
    public class FileConverterModel : IFileConverterModel
	{
		public Queue<string> Files { get; set; }
		private string _fileName;
		private int _percentOfWords;
		private int _percentOfSymbols;
		private int _left;
		public string ConnectionString { get; }
		public bool TransliterationState { get; set; }
        public bool Stop { get; set; }

		public int Left
		{
			get => _left;
			set
			{
				_left = value;
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

	    public FileConverterModel()
		{
			ConnectionString = ConfigurationManager.ConnectionStrings["LiteDatabaseConnection"].ConnectionString;
			Files = new Queue<string>();
		}

		public event PropertyChangedEventHandler PropertyChanged;
		public void OnPropertyChanged([CallerMemberName]string prop = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
		}

	    public void StopTransliteration()
	    {
	        Stop = true;
	    }

        public void AddFiles(string[] files)
		{
			foreach (var f in files)
			{
				Files.Enqueue(f);
			}
			Left = Files.Count;
		}

		public bool TranslitFiles(string[] files, bool? ignoreSelectedText)
		{
			AddFiles(files);

			TransliterationState = true;

			while (Files.Count > 0)
			{
			    if (Stop)
			    {
			        Files.Clear();
			        Stop = false;
                    TransliterationState = false;
			        return false;
			    }

				Left = Files.Count;
				var file = FileName = Files.Dequeue();

				// Получение информации о файле
				var fileInfo = new FileInfo(file);

				if (fileInfo.Length == 0) continue;

				if (file.ToLower().EndsWith(".doc") || file.ToLower().EndsWith(".docx"))
				{
					TranslitWordFile(file, ignoreSelectedText);
				}
				else if (file.ToLower().EndsWith(".xls") || file.ToLower().EndsWith(".xlsx"))
				{
					TranslitExcelFile(file);
				}
				else if (file.ToLower().EndsWith(".ppt") || file.ToLower().EndsWith(".pptx"))
				{
					TranslitPowerPointFile(file);
				}
				else if (file.ToLower().EndsWith(".txt"))
				{
					TranslitTxtFile(file);
				}
				else if (file.ToLower().EndsWith(".pdf") || file.ToLower().EndsWith(".rtf"))
				{
					TranslitPdfOrRtfFile(file);
				}
			}

			TransliterationState = false;
		    return true;
		}

		// Транслитерация файла Word
		private void TranslitWordFile(string filename, bool? ignoreSelectedText)
		{
			var isConverted = false;

			var translitFileName = filename;

			if (filename.ToLower().EndsWith(".doc"))
			{
				translitFileName = ConvertDocToDocx(filename);
				isConverted = true;
			}

			// Распаковка документа и получение расположения распакованной папки
			var tempFolder = UnZipFileToTempFolder(translitFileName);

			// Определяем путь к xml файлу распакованного документа
			var xmlFile = tempFolder + @"\word\document.xml";

			// Открваем документ xml
			var doc = XDocument.Load(xmlFile, LoadOptions.PreserveWhitespace);

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
								            n.Descendants().Where(r => r.Name.LocalName == "highlight").ToList().Count == 0)
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

			doc.Save(xmlFile);

			if (isConverted)
			{
				File.Delete(translitFileName);
			}

			CreateNewFileFromDirectory(tempFolder, filename, isConverted);
		}

		// Транслитерация файла Excel
		private void TranslitExcelFile(string filename)
		{
			var isConverted = false;

			var translitFileName = filename;

			if (filename.ToLower().EndsWith(".xls"))
			{
				translitFileName = ConvertXlsToXlsx(filename);
				isConverted = true;
			}

			// Распаковка документа и получение расположения распакованной папки
			var tempFolder = UnZipFileToTempFolder(translitFileName);

			// Определяем путь к xml файлу распакованного документа
			var xmlFile = tempFolder + @"\xl\sharedStrings.xml";

			// Открваем документ xml
			var doc = XDocument.Load(xmlFile, LoadOptions.PreserveWhitespace);

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

			doc.Save(xmlFile);

			if (isConverted)
			{
				File.Delete(translitFileName);
			}

			CreateNewFileFromDirectory(tempFolder, filename, isConverted);
		}

		// Транслитерация файла Power Point
		private void TranslitPowerPointFile(string filename)
		{
			var isConverted = false;

			var translitFileName = filename;

			if (filename.ToLower().EndsWith(".ppt"))
			{
				translitFileName = ConvertPptToPptx(filename);
				isConverted = true;
			}

			// Распаковка документа и получение расположения распакованной папки
			var tempFolder = UnZipFileToTempFolder(translitFileName);

			var folderWithXml = tempFolder + @"\ppt\slides";

			// Обработка отсутствия слайдов в презентации
			if (!Directory.Exists(folderWithXml))
			{
				Directory.Delete(tempFolder, true);
				return;
			}

			// Получаем пути всех нескрытых файлов с расширением xml
			var xmlFiles = new DirectoryInfo(folderWithXml).EnumerateFiles()
				.Where(f => (f.Attributes & FileAttributes.Hidden) == 0 && f.Extension == ".xml")
				.Select(f => f.FullName).ToArray();

			foreach (var xmlFile in xmlFiles)
			{
				// Открваем документ xml
				var doc = XDocument.Load(xmlFile, LoadOptions.PreserveWhitespace);

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

				doc.Save(xmlFile);
			}

			if (isConverted)
			{
				File.Delete(translitFileName);
			}

			CreateNewFileFromDirectory(tempFolder, filename, isConverted);
		}

		// Транслитерация текстового документа
		private void TranslitTxtFile(string filename)
		{
			var text = File.ReadAllText(filename);
			
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

						text = text.Replace(cyryllic, latin);
					}

					// Высчитываем процент текущего прогресса
					PercentOfWords = i * 100 / (words.Length - 1);
				}

				var symbols = db.GetCollection<Symbol>("Symbols").FindAll().ToArray();

				for (var i = 0; i < symbols.Length; i++)
				{
					text = text.Replace(symbols[i].Cyryllic, symbols[i].Latin);

					PercentOfSymbols = i * 100 / (symbols.Length - 1);
				}
			}

			if (Settings.Default.AutoSave)
			{
				var fileInfo = new FileInfo(filename);

				// Получение данных о новом файле
				var directoryName = fileInfo.DirectoryName;
				var fileName = fileInfo.Name.Substring(0, fileInfo.Name.Length - fileInfo.Extension.Length);
				var label = $" ({GetRes("FileNameLatin")})";
				var extension = fileInfo.Extension;

				// Комбинирование данных
				var newFileName = $@"{directoryName}\{fileName}{label}{extension}";

				File.WriteAllText(newFileName, text);
			}
			else
			{
				File.WriteAllText(filename, text);
			}
			
		}

		// Транслитерация файла PDF и RTF
		private void TranslitPdfOrRtfFile(string filename)
		{
			var dc = DocumentCore.Load(filename);

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

						var regex = new Regex(cyryllic, RegexOptions.None);

						foreach (var item in dc.Content.Find(regex).Reverse())
						{
							item.Replace(latin);
						}
					}

					// Высчитываем процент текущего прогресса
					PercentOfWords = i * 100 / (words.Length - 1);
				}

				var symbols = db.GetCollection<Symbol>("Symbols").FindAll().ToArray();

				for (var i = 0; i < symbols.Length; i++)
				{
					var regex = new Regex(symbols[i].Cyryllic, RegexOptions.None);

					foreach (var item in dc.Content.Find(regex).Reverse())
					{
						item.Replace(symbols[i].Latin);
					}

					PercentOfSymbols = i * 100 / (symbols.Length - 1);
				}
			}

			if (Settings.Default.AutoSave)
			{
				var fileInfo = new FileInfo(filename);

				// Получение данных о новом файле
				var directoryName = fileInfo.DirectoryName;
				var fileName = fileInfo.Name.Substring(0, fileInfo.Name.Length - fileInfo.Extension.Length);
				var label = $" ({GetRes("FileNameLatin")})";
				var extension = fileInfo.Extension;

				// Комбинирование данных
				var newFileName = $@"{directoryName}\{fileName}{label}{extension}";

				switch (extension)
				{
					case ".pdf":
						dc.Save(newFileName, new PdfSaveOptions());
						break;
					case ".rtf":
						dc.Save(newFileName, new RtfSaveOptions());
						break;
				}
			}
			else
			{
				if (filename.ToLower().EndsWith(".pdf"))
				{
					dc.Save(filename, new PdfSaveOptions());
				}
				else if (filename.ToLower().EndsWith(".rtf"))
				{
					dc.Save(filename, new RtfSaveOptions());
				}
			}
		}

		private static string UnZipFileToTempFolder(string filename)
		{
			// Путь временной папки
			var tempFolder = $@"{Path.GetTempPath()}Translit\{Guid.NewGuid()}";

			// Распаковка документа во временную папку
			using (var archive = ZipFile.OpenRead(filename))
			{
				archive.ExtractToDirectory(tempFolder);
			}

			return tempFolder;
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

			if (isConverted)
			{
				newFileName += "x";
			}

			if (File.Exists(newFileName))
			{
				File.Delete(newFileName);
			}

			ZipFile.CreateFromDirectory(folder, newFileName);

			// Удаление временной папки
			try
			{
				Directory.Delete(folder, true);
			}
			catch (Exception)
			{
				// ignored
			}

			// Если автосохранение включено или файл был конвертирован в новый формат
			if (Settings.Default.AutoSave || isConverted) return;

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

		public string ConvertDocToDocx(string filename)
		{
			// Получение информации о файле
			var fileInfo = new FileInfo(filename);

			string newFileName;

			do
			{
				newFileName = $@"{fileInfo.DirectoryName}\{Guid.NewGuid()}{fileInfo.Extension}x";
			} while (File.Exists(newFileName));

			var word = new Microsoft.Office.Interop.Word.Application();
			var document = word.Documents.Open(filename);
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
			// Получение информации о файле
			var fileInfo = new FileInfo(filename);

			string newFileName;

			do
			{
				newFileName = $@"{fileInfo.DirectoryName}\{Guid.NewGuid()}{fileInfo.Extension}x";
			} while (File.Exists(newFileName));

			var excel = new Microsoft.Office.Interop.Excel.Application();
			var workbook = excel.Workbooks.Open(filename);
			workbook.SaveAs(newFileName, XlFileFormat.xlOpenXMLWorkbook);
			excel.ActiveWorkbook.Close();
			excel.Quit();

			if (!Settings.Default.AutoSave)
			{
				File.Delete(filename);
			}

			return newFileName;
		}

		private string ConvertPptToPptx(string filename)
		{
			// Получение информации о файле
			var fileInfo = new FileInfo(filename);

			string newFileName;

			do
			{
				newFileName = $@"{fileInfo.DirectoryName}\{Guid.NewGuid()}{fileInfo.Extension}x";
			} while (File.Exists(newFileName));

			var powerPoint = new Microsoft.Office.Interop.PowerPoint.Application();
			var presentation = powerPoint.Presentations.Open(filename, WithWindow: MsoTriState.msoFalse);
			presentation.SaveAs(newFileName, PpSaveAsFileType.ppSaveAsOpenXMLPresentation);

			// Закрывает все другие открытые презентации
			//powerPoint.ActivePresentation.Close();
			//powerPoint.Quit();

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