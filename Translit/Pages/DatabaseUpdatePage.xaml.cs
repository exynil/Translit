using LiteDB;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Translit.Entity;

namespace Translit.Pages
{
	public partial class DatabaseUpdatePage
	{
		public Snackbar SnackbarInfo { get; set; }
		public string ConnectionString { get; set; }

		public DatabaseUpdatePage(Snackbar snackbar)
		{
			InitializeComponent();
			SnackbarInfo = snackbar;
			ConnectionString = ConfigurationManager.ConnectionStrings["LiteDatabaseConnection"].ConnectionString;
		}

		// Нажатие кнопки обновления базы
		private async void ButtonDatabaseUpdate_OnClick(object sender, RoutedEventArgs e)
		{
			ButtonDatabaseUpdate.Click -= ButtonDatabaseUpdate_OnClick;
			ButtonDatabaseUpdate.SetValue(ButtonProgressAssist.IsIndeterminateProperty, true);
			ButtonDatabaseUpdate.Content = GetRes("ButtonLoading");

			// Сбрасываем текущее значение индикатора програсса
			ProgressBarSymbolLoading.Value = 0;
			// Сбрасываем текущее значение индикатора прогресса
			ProgressBarWordLoading.Value = 0;

			await Task.Factory.StartNew(() =>
			{
				int wordsCount;
				int symbolsCount;
				string[] links = { @"http://translit.osmium.kz/api/symbol", @"http://translit.osmium.kz/api/word" };
				var request = new HttpWebRequest[2];
				request[0] = (HttpWebRequest)WebRequest.Create(links[0]);
				request[1] = (HttpWebRequest)WebRequest.Create(links[1]);
				var response = new HttpWebResponse[2];

				// Пытаем получить ответ от сервера
				try
				{
					response[0] = (HttpWebResponse)request[0].GetResponse();
					response[1] = (HttpWebResponse)request[1].GetResponse();
				}
				catch (Exception)
				{
					// Уведомляем об отсутвие интернета или плохом соединении
					SnackbarInfo.Dispatcher.Invoke(
						() => { SnackbarInfo.MessageQueue.Enqueue(GetRes("SnackBarBadInternetConnection")); },
						DispatcherPriority.Background);
					return;
				}

				List<Symbol> listSymbols;
				List<Word> listWords;

				// Загружаем первый ответ сервера в базу
				var responseSrteam = response[0].GetResponseStream();
				using (var stream = new StreamReader(responseSrteam ?? throw new InvalidOperationException(), Encoding.UTF8))
				{
					// Десериализуем ответ
					listSymbols = JsonConvert.DeserializeObject<List<Symbol>>(stream.ReadToEnd());
				}

				// Загружаем второй ответ сервера в базу
				responseSrteam = response[1].GetResponseStream();
				using (var stream = new StreamReader(responseSrteam ?? throw new InvalidOperationException(), Encoding.UTF8))
				{
					// Десериализуем ответ
					listWords = JsonConvert.DeserializeObject<List<Word>>(stream.ReadToEnd());
				}

				StackPanelProgress.Dispatcher.Invoke(() => { StackPanelProgress.Visibility = Visibility.Visible; },
					DispatcherPriority.Background);

				// Удаляем локальную базу
				File.Delete(ConnectionString);

				// Подлключаемся к локальной базе
				using (var db = new LiteDatabase(ConnectionString))
				{
					// Создаем коллекцию
					var symbols = db.GetCollection<Symbol>("Symbols");
					// Перебираем полученный ответ от сервера и дабавляем каждое исключение в базу
					for (int[] i = { 0 }; i[0] < listSymbols.Count; i[0]++)
					{
						// Высчитываем процент выполнения
						long percent = i[0] * 100 / (listSymbols.Count - 1);
						// Изменяем значение прогресса
						ProgressBarSymbolLoading.Dispatcher.Invoke(() => ProgressBarSymbolLoading.Value = percent,
							DispatcherPriority.Background);
						// Выводим информацию о текущем индексе
						TextBlockSymbolLoading.Dispatcher.Invoke(
							() =>
							{
								TextBlockSymbolLoading.Text = GetRes("TextBlockCharacterLoading") + ": " + (i[0] + 1) + "/" + listSymbols.Count;
							}, DispatcherPriority.Background);
						// Добавляем символ в базу
						symbols.Upsert(listSymbols[i[0]]);
					}

					// Запоминаем кол-во символов
					symbolsCount = listSymbols.Count;

					// Создаем коллекцию
					var words = db.GetCollection<Word>("Words");
					// Перебираем полученный ответ сервера и добавляем каждое исключение в базу
					for (int[] i = { 0 }; i[0] < listWords.Count; i[0]++)
					{
						//Высчитываем процент выполенения
						long percent = i[0] * 100 / (listWords.Count - 1);
						// Изменяем значение прогресса
						ProgressBarWordLoading.Dispatcher.Invoke(() => ProgressBarWordLoading.Value = percent,
							DispatcherPriority.Background);
						// Выводим информацию о текущем индексе
						TextBlockWordLoading.Dispatcher.Invoke(
							() =>
							{
								TextBlockWordLoading.Text = GetRes("TextBlockLoadingExceptions") + ": " + (i[0] + 1) + "/" + listWords.Count;
							}, DispatcherPriority.Background);
						// Добавляем исключение в базу
						words.Upsert(listWords[i[0]]);
					}

					// Запоминаем кол-во исключений
					wordsCount = listWords.Count;
				}

				// Уведомляем об успешном завершении обновления
				SnackbarInfo.Dispatcher.Invoke(() => SnackbarInfo.MessageQueue.Enqueue(GetRes("SnackBarUpdateCompleted")),
					DispatcherPriority.Background);

				// Получаем информацию о файле
				var fileInfo = new FileInfo(ConnectionString);
				// Выводим информацию о размере файла
				TextBlockDatabase.Dispatcher.Invoke(() => TextBlockDatabase.Text = GetRes("TextBlockDatabase"),
					DispatcherPriority.Background);
				if (fileInfo.Length < 1048576)
				{
					TextBlockDatabaseSize.Dispatcher.Invoke(() =>
					{
						var text = GetRes("TextBlockDatabaseSize") + ": " + fileInfo.Length / 1024 + GetRes("FileKb");
						TextBlockDatabaseSize.Text = text;
					}, DispatcherPriority.Background);
				}
				else
				{
					TextBlockDatabaseSize.Dispatcher.Invoke(() =>
					{
						var text = GetRes("TextBlockDatabaseSize") + ": " + fileInfo.Length / 1024 / 1024 + GetRes("FileMb");
						TextBlockDatabaseSize.Text = text;
					}, DispatcherPriority.Background);
				}

				// Выводимо информацию о кол-ве символов
				TextBlockSymbolsCount.Dispatcher.Invoke(
					() => { TextBlockSymbolsCount.Text = GetRes("TextBlockAmountOfCharacters") + ": " + symbolsCount; },
					DispatcherPriority.Background);

				// Выводим информацию о кол-ве исключений
				TextBlockWordsCount.Dispatcher.Invoke(
					() => { TextBlockWordsCount.Text = GetRes("TextBlockAmountOfWords") + ": " + wordsCount; },
					DispatcherPriority.Background);
				// Выводим информацию о дате последнего обновления
				TextBlockLastLastUpdate.Dispatcher.Invoke(
					() => { TextBlockLastLastUpdate.Text = GetRes("TextBlockLastUpdate") + ": " + fileInfo.LastWriteTime; },
					DispatcherPriority.Background);
			});
			ButtonDatabaseUpdate.SetValue(ButtonProgressAssist.IsIndeterminateProperty, false);
			ButtonDatabaseUpdate.Content = GetRes("ButtonUpdate");
			StackPanelProgress.Visibility = Visibility.Hidden;
			ButtonDatabaseUpdate.Click += ButtonDatabaseUpdate_OnClick;
		}

		// Первоначальная загрузка страницы
		private void DatabaseUpdatePage_OnLoaded(object sender, RoutedEventArgs e)
		{
			// Если файл найден
			if (File.Exists(ConnectionString))
			{
				// Получаем информацию о файле
				var fileInfo = new FileInfo(ConnectionString);

				// Подключаемся к локальной базе
				using (var db = new LiteDatabase(ConnectionString))
				{
					// Получаем коллекции
					var words = db.GetCollection<Word>("Words");
					var symbols = db.GetCollection<Symbol>("Symbols");
					TextBlockDatabase.Text = GetRes("TextBlockDatabase");
					// Выводим информацию о размере файла
					if (fileInfo.Length < 1048576)
					{
						string text = GetRes("TextBlockDatabaseSize") + ": " + fileInfo.Length / 1024 + GetRes("FileKb");
						TextBlockDatabaseSize.Text = text;
					}
					else
					{
						string text = GetRes("TextBlockDatabaseSize") + ": " + fileInfo.Length / 1024 / 1024 + GetRes("FileMb");
						TextBlockDatabaseSize.Text = text;
					}

					TextBlockSymbolsCount.Text = GetRes("TextBlockAmountOfCharacters") + ": " + symbols.Count();
					TextBlockWordsCount.Text = GetRes("TextBlockAmountOfWords") + ": " + words.Count();
					TextBlockLastLastUpdate.Text = GetRes("TextBlockLastUpdate") + ": " + fileInfo.LastWriteTime;
				}
			}
			else
			{
				TextBlockDatabase.Text = GetRes("TextBlockDatabaseNotFound");
			}
		}

		private string GetRes(string key)
		{
			return Application.Current.Resources[key].ToString();
		}
	}
}