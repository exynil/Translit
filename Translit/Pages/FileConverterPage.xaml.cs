using System;
using System.Configuration;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using MicrosoftWord = Microsoft.Office.Interop.Word;
using System.Windows.Forms;
using System.IO;
using System.Windows.Controls;
using LiteDB;
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
	            TextBlockInfo.Visibility = Visibility.Hidden;
				// Транслитерация выбранного файла
				Translit(dlg.FileName);
				// Скрываем прогресс и показываем информацию
	            ProgressBarExceptions.Visibility = Visibility.Hidden;
	            TextBlockExceptions.Visibility = Visibility.Hidden;
	            ProgressBarDocument.Visibility = Visibility.Hidden;
	            TextBlockDocument.Visibility = Visibility.Hidden;
	            TextBlockInfo.Visibility = Visibility.Visible;
				TextBlockInfo.SetResourceReference(TextBlock.TextProperty, "TransliterationCompleted");
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
	                var noHiddenDocFiles = new DirectoryInfo(fbd.SelectedPath).EnumerateFiles().Where(f => (f.Attributes & FileAttributes.Hidden) == 0 && (f.Extension == ".doc" || f.Extension == ".docx")).ToArray();

					// Извлекаем полный путь всех файлов
					string[] files = noHiddenDocFiles.Select(f=>f.FullName).ToArray();

					// Манипуляции с индикатором прогресса
	                ProgressBarDocuments.Value = 0;
	                ProgressBarDocuments.Maximum = files.Length - 1;
	                ProgressBarDocuments.Visibility = Visibility.Visible;
	                TextBlockInfo.Visibility = Visibility.Hidden;

					// Перебор и транслитерация всех файлов
					for (int[] i = {0}; i[0] < files.Length; i[0]++)
		            {
			            TextBlockDocumetns.Text = System.Windows.Application.Current.Resources["TextBlockDocuments"] +  ": " + (i[0] + 1) + "/" + files.Length;
			            ProgressBarDocuments.Dispatcher.Invoke(() => ProgressBarDocuments.Value = i[0], DispatcherPriority.Background);
			            Translit(files[i[0]]);
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

		// Транслитерация
		private void Translit(string filename)
		{
			// Копируем путь нашего файла
			string copyFileName = filename;
			// Если автосохранение включено, выполняем резервную копию
	        if (Settings.Default.AutoSave)
	        {
		        string extension = "";
				// Перебираем пусть с конца
		        for (int i = copyFileName.Length - 1; i > 0; i--)
		        {
					// В extension прибавлям элемент строки под текущим индексом
			        extension = copyFileName[i] + extension;
					// Если в переборе встречается точка, выполняем следующие действия
			        if (copyFileName[i] == '.')
			        {
						// Извлекаем подстроку пол до точки
				        copyFileName = copyFileName.Substring(0, i);
						// Добавляем уникальное имя и расширение файла
				        copyFileName += " (" + System.Windows.Application.Current.Resources["FileNameLatin"] + ")" + extension;
						break;
			        }
		        }
				// Копируем файл
				File.Copy(filename, copyFileName, true);
	        }

			MicrosoftWord.Application app = new MicrosoftWord.Application();
			app.Documents.Open(copyFileName);
			MicrosoftWord.Find find = app.Selection.Find;

			using (LiteDatabase db = new LiteDatabase(ConfigurationManager.ConnectionStrings["LiteDatabaseConnection"].ConnectionString))
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
						} else if (j == 2)
						{
							cyryllic = cyryllic.First().ToString().ToUpper() + cyryllic.Substring(1);
							latin = latin.First().ToString().ToUpper() + latin.Substring(1);
						}

						find.Text = cyryllic;
						find.Replacement.Text = latin;
						find.Execute(FindText: Type.Missing,
							MatchCase: true,
							MatchWholeWord: false,
							MatchWildcards: false,
							MatchSoundsLike: Type.Missing,
							MatchAllWordForms: false,
							Forward: true,
							Wrap: MicrosoftWord.WdFindWrap.wdFindContinue,
							Format: false,
							ReplaceWith: Type.Missing,
							Replace: MicrosoftWord.WdReplace.wdReplaceAll);
					}
					
					// Высчитываем процент текущего прогресса
					long percent = i * 100 / (words.Length - 1);
					// Выводим результат
					TextBlockExceptions.Text = System.Windows.Application.Current.Resources["TextBlockTransliterationOfExceptionWords"] + ": " + percent + "%";
					// Изменяем прогресс
					ProgressBarExceptions.Dispatcher.Invoke(() => ProgressBarExceptions.Value = percent, DispatcherPriority.Background);
				}

				var symbols = db.GetCollection<Symbol>("Symbols").FindAll().ToArray();

				ProgressBarDocument.Value = 0;
				ProgressBarDocument.Visibility = Visibility.Visible;
		        TextBlockDocument.Visibility = Visibility.Visible;

				for (int i = 0; i < symbols.Length; i++)
				{
					find.Text = symbols[i].Cyryllic;
					find.Replacement.Text = symbols[i].Latin;
					find.Execute(FindText: Type.Missing,
						MatchCase: true,
						MatchWholeWord: false,
						MatchWildcards: false,
						MatchSoundsLike: Type.Missing,
						MatchAllWordForms: false,
						Forward: true,
						Wrap: MicrosoftWord.WdFindWrap.wdFindContinue,
						Format: false,
						ReplaceWith: Type.Missing,
						Replace: MicrosoftWord.WdReplace.wdReplaceAll);

					long percent = i * 100 / (symbols.Length - 1);
					TextBlockDocument.Text = System.Windows.Application.Current.Resources["TextBlockCharacterTransliteration"] + ": " + percent + "%";
					ProgressBarDocument.Dispatcher.Invoke(() => ProgressBarDocument.Value = percent, DispatcherPriority.Background);
				}
			}
			app.ActiveDocument.Save();
			app.ActiveDocument.Close();
			app.Quit();
		}
	}
}