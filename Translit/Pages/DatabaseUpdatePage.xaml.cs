using LiteDB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using MaterialDesignThemes.Wpf;
using Translit.Properties;
using System.Configuration;
using Translit.Entity;

namespace Translit.Pages
{
	public partial class DatabaseUpdatePage
	{
		private readonly Snackbar _snackbar;
		public DatabaseUpdatePage(Snackbar snackbar)
		{
			InitializeComponent();
			_snackbar = snackbar;
		}
		// Асинхронный показ уведомления
		private async Task ShowAsyncNotification(string resourceName)
		{
			await Task.Factory.StartNew(() => { }).ContinueWith(t => { _snackbar.MessageQueue.Enqueue(Application.Current.Resources[resourceName]); }, TaskScheduler.FromCurrentSynchronizationContext());
		}

		// Показ уведомления
		private void ShowNotification(string resourceName)
		{
			Task.Factory.StartNew(() => { }).ContinueWith(t => { _snackbar.MessageQueue.Enqueue(Application.Current.Resources[resourceName]); }, TaskScheduler.FromCurrentSynchronizationContext());
		}

		// Нажатие кнопки обновления базы
		private void ButtonDatabaseUpdate_OnClick(object sender, RoutedEventArgs e)
		{
			// Делаем кнопку "Обновить" не активной
			((Button) sender).IsEnabled = false;

			Dispatcher.InvokeAsync(async () =>
			{
				int wordsCount;
				int symbolsCount;

				string[] links =
				{
					@"http://translit.osmium.kz/api/symbol",
					@"http://translit.osmium.kz/api/word"
				};
				HttpWebRequest[] request = new HttpWebRequest[2];
				request[0] = (HttpWebRequest)WebRequest.Create(links[0]);
				request[1] = (HttpWebRequest)WebRequest.Create(links[1]);
				HttpWebResponse[] response = new HttpWebResponse[2];
				// Пытаем получить ответ от сервера
				try
				{
					response[0] = (HttpWebResponse)request[0].GetResponse();
					response[1] = (HttpWebResponse)request[1].GetResponse();
				}
				catch (Exception)
				{
					await ShowAsyncNotification("SnackBarBadInternetConnection");
					((Button)sender).IsEnabled = true;
					return;
				}

				// Загружаем первый ответ сервера в базу
				using (StreamReader stream = new StreamReader(response[0].GetResponseStream() ?? throw new InvalidOperationException(), Encoding.UTF8))
				{
					// Получаем строку ответа
					string dataJson = stream.ReadToEnd();
					// Десериализуем ответ
					var list = JsonConvert.DeserializeObject<List<Symbol>>(dataJson);
					// Сбрасываем текущее значение индикатора програсса
					ProgressBarSymbolLoading.Value = 0;
					TextBlockSymbolLoading.Visibility = Visibility.Visible;
					ProgressBarSymbolLoading.Visibility = Visibility.Visible;
					TextBlockWordLoading.Visibility = Visibility.Visible;
					ProgressBarWordLoading.Visibility = Visibility.Visible;
					// Подлключаемся к локальной базе
					using (LiteDatabase db = new LiteDatabase(ConfigurationManager.ConnectionStrings["LiteDatabaseConnection"].ConnectionString))
					{
						// Удаляем существующую коллекцию
						db.DropCollection("Symbols");
						// Создаем коллекцию
						var symbols = db.GetCollection<Symbol>("Symbols");
						// Перебираем полученный ответ от сервера и дабавляем каждое исключение в базу
						for (int i = 0; i < list.Count; i++)
						{
							// Высчитываем процент выполнения
							long percent = i * 100 / (list.Count - 1);
							// Изменяем значение прогресса
							ProgressBarSymbolLoading.Dispatcher.Invoke(() => ProgressBarSymbolLoading.Value = percent, DispatcherPriority.Background);
							// Выводим информацию о текущем индексе
							TextBlockSymbolLoading.Text = Application.Current.Resources["TextBlockCharacterLoading"] + ": " + (i + 1) + "/" + list.Count;
							// Добавляем символ в базу
							symbols.Upsert(list[i]);
						}
						// Запоминаем кол-во символов
						symbolsCount = list.Count;
					}
				}

				// Загружаем второй ответ сервера в базу
				using (StreamReader stream = new StreamReader(response[1].GetResponseStream() ?? throw new InvalidOperationException(), Encoding.UTF8))
				{
					// Получаем строку ответа
					string dataJson = stream.ReadToEnd();
					// Десериализуем ответ
					var list = JsonConvert.DeserializeObject<List<Word>>(dataJson);
					// Сбрасываем текущее значение индикатора прогресса
					ProgressBarWordLoading.Value = 0;
					// Подключаемся к локальной базе
					using (LiteDatabase db = new LiteDatabase(ConfigurationManager.ConnectionStrings["LiteDatabaseConnection"].ConnectionString))
					{
						// Удаляем существующую коллекцию
						db.DropCollection("Words");
						// Создаем коллекцию
						var words = db.GetCollection<Word>("Words");
						// Перебираем полученный ответ сервера и добавляем каждое исключение в базу
						for (int i = 0; i < list.Count; i++)
						{
							//Высчитываем процент выполенения
							long percent = i * 100 / (list.Count - 1);
							// Изменяем значение прогресса
							ProgressBarWordLoading.Dispatcher.Invoke(() => ProgressBarWordLoading.Value = percent, DispatcherPriority.Background);
							// Выводим информацию о текущем индексе
							TextBlockWordLoading.Text = Application.Current.Resources["TextBlockLoadingExceptions"] + ": " + (i + 1) + "/" + list.Count;
							// Добавляем исключение в базу
							words.Upsert(list[i]);
						}
						// Запоминаем кол-во исключений
						wordsCount = list.Count;
					}
				}
				TextBlockSymbolLoading.Visibility = Visibility.Collapsed;
				ProgressBarSymbolLoading.Visibility = Visibility.Collapsed;
				TextBlockWordLoading.Visibility = Visibility.Collapsed;
				ProgressBarWordLoading.Visibility = Visibility.Collapsed;
				// Уведомляем об успешном завершении обновления
				await ShowAsyncNotification("SnackBarUpdateCompleted");
				// Получаем текущую дату
				DateTime dateTime = DateTime.Now;
				// Получаем информацию о файле
				FileInfo fileInfo = new FileInfo(ConfigurationManager.ConnectionStrings["LiteDatabaseConnection"].ConnectionString);
				// Выводим информацию о размере файла
				if (fileInfo.Length < 1048576)
				{
					TextBlockDatabaseSize.Text = Application.Current.Resources["TextBlockDatabaseSize"] + ": " + fileInfo.Length / 1024 + Application.Current.Resources["FileKb"];
				}
				else
				{
					TextBlockDatabaseSize.Text = Application.Current.Resources["TextBlockDatabaseSize"] + ": " + fileInfo.Length / 1024 / 1024 + Application.Current.Resources["FileMb"];
				}
				// Выводимо информацию о кол-ве символов
				TextBlockSymbolsCount.Text = Application.Current.Resources["TextBlockAmountOfCharacters"] + ": " + symbolsCount;
				// Выводим информацию о кол-ве исключений
				TextBlockWordsCount.Text = Application.Current.Resources["TextBlockAmountOfWords"] + ": " + wordsCount;
				// Выводим информацию о дате последнего обновления
				TextBlockLastUpdateDate.Text = Application.Current.Resources["TextBlockLastUpdate"] + ": " + dateTime;
				// Сохраянем дату последнего обновления
				Settings.Default.DatabaseUpdateDate = dateTime;
				// Делаем кнопку "Обновить" активной
				((Button)sender).IsEnabled = true;
			});
		}

		// Первоначальная загрузка страницы
		private void DatabaseUpdatePage_OnLoaded(object sender, RoutedEventArgs e)
		{
			// Если файл найден
			if (File.Exists(ConfigurationManager.ConnectionStrings["LiteDatabaseConnection"].ConnectionString))
			{
				// Получаем информацию о файле
				FileInfo fileInfo = new FileInfo(ConfigurationManager.ConnectionStrings["LiteDatabaseConnection"].ConnectionString);

				// Подключаемся к локальной базе
				using (LiteDatabase db = new LiteDatabase(ConfigurationManager.ConnectionStrings["LiteDatabaseConnection"].ConnectionString))
				{
					// Получаем коллекции
					var words = db.GetCollection<Word>("Words");
					var symbols = db.GetCollection<Symbol>("Symbols");

					// Выводим информацию об локальной базе
					// Выводим информацию о размере файла
					if (fileInfo.Length < 1048576)
					{
						TextBlockDatabaseSize.Text = Application.Current.Resources["TextBlockDatabaseSize"] + ": " + fileInfo.Length / 1024 + Application.Current.Resources["FileKb"];
					}
					else
					{
						TextBlockDatabaseSize.Text = Application.Current.Resources["TextBlockDatabaseSize"] + ": " + fileInfo.Length / 1024 / 1024 + Application.Current.Resources["FileMb"];
					}
					TextBlockSymbolsCount.Text = Application.Current.Resources["TextBlockAmountOfCharacters"] + ": " + symbols.Count();
					TextBlockWordsCount.Text = Application.Current.Resources["TextBlockAmountOfWords"] + ": " + words.Count();
					TextBlockLastUpdateDate.Text = Application.Current.Resources["TextBlockLastUpdate"] + ": " + Settings.Default.DatabaseUpdateDate;
				}
			}
			else
			{
				ShowNotification("SnackBarLocalDatabaseNotFound");
			}
		}
	}
}