using LiteDB;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Translit.Entity;
using Translit.Windows;

namespace Translit.Pages
{
	public partial class WordsEditorPage
	{
		public User User;
		public Snackbar SnackbarInfo { get; set; }

		public WordsEditorPage(Snackbar snackbar)
		{
			InitializeComponent();
			SnackbarInfo = snackbar;
		}

		private void Page_Loaded(object sender, RoutedEventArgs e)
		{
			UpdateDataGridWords();
		}

		// Асинхронный показ уведомления
		private async Task ShowAsyncNotification(string resourceName)
		{
			await Task.Factory.StartNew(() => { }).ContinueWith(t => { SnackbarInfo.MessageQueue.Enqueue(Application.Current.Resources[resourceName]); }, TaskScheduler.FromCurrentSynchronizationContext());
		}

		// Показ уведомления
		private void ShowNotification(string resourceName)
		{
			Task.Factory.StartNew(() => { }).ContinueWith(t => { SnackbarInfo.MessageQueue.Enqueue(Application.Current.Resources[resourceName]); }, TaskScheduler.FromCurrentSynchronizationContext());
		}

		// Обновление списка
		private void UpdateDataGridWords()
		{
			using (LiteDatabase db = new LiteDatabase(ConfigurationManager.ConnectionStrings["LiteDatabaseConnection"].ConnectionString))
			{
				DataGridWords.ItemsSource = db.GetCollection<Word>("Words").FindAll();
			}
		}

		// Нажатие кнопки добавления нового слова
		private void ButtonAddWord_OnClick(object sender, RoutedEventArgs e)
		{
			// Создаем диалоговое окно
			AddDialogWindow addDialogWindow = new AddDialogWindow();
			// Ожидаем ответ пользователя
			if (addDialogWindow.ShowDialog() == true)
			{
				string cyryllic = addDialogWindow.TextBoxAddCyryllic.Text;
				string latin = addDialogWindow.TextBoxAddLatin.Text;

				// Закрываем диалоговое окно
				addDialogWindow.Close();

				if (cyryllic.Length > 30 || latin.Length > 40)
				{
					ShowNotification("SnackBarNotAllowedLength");
					return;
				}

				string link = "http://translit.osmium.kz/api/word?";

				HttpClient client = new HttpClient();

				// Создаем параметры
				var content = new FormUrlEncodedContent(new Dictionary<string, string>
				{
					{"token", User.Token},
					{"cyrl", addDialogWindow.TextBoxAddCyryllic.Text },
					{"latn", addDialogWindow.TextBoxAddLatin.Text}
				});

				Dispatcher.InvokeAsync(async () =>
				{
					// Выполняем запрос
					var response = await client.PostAsync(link, content);

					// Полученный ответ десериализуем в объект Word
					Word addedWord = JsonConvert.DeserializeObject<Word>(await response.Content.ReadAsStringAsync());

					// Выполняем следующие действия взамисимости от ответа сервера
					if (response.StatusCode == HttpStatusCode.Created)
					{
						// Добавляем слово в локальную базу
						using (LiteDatabase db = new LiteDatabase(ConfigurationManager.ConnectionStrings["LiteDatabaseConnection"].ConnectionString))
						{
							var words = db.GetCollection<Word>("Words");
							words.Insert(addedWord);
							UpdateDataGridWords();
							await ShowAsyncNotification("SnackBarRecordSuccessfullyAdded");
						}
					}
					else if (response.StatusCode == HttpStatusCode.InternalServerError)
					{
						await ShowAsyncNotification("SnackBarServerSideError");
					}
					else if (response.StatusCode == HttpStatusCode.Conflict)
					{
						await ShowAsyncNotification("SnackBarTheWordIsAlreadyInTheDatabase");
					}
					else
					{
						await ShowAsyncNotification("SnackBarError");
					}
				});
			}
			else
			{
				ShowNotification("SnackBarAddingEntryCanceled");
			}
		}

		// Нажатие кнопки изменения слова
		private void ButtonEditWord_OnClick(object sender, RoutedEventArgs e)
		{
			int id = ((Word)((Button)sender).DataContext).Id;
			string cyryllic = ((Word)((Button)sender).DataContext).Cyryllic;
			string latin = ((Word)((Button)sender).DataContext).Latin;
			// Создаем диалоговое окно
			EditDialogWindow editDialogWindow = new EditDialogWindow(cyryllic, latin);
			// Ожидаем ответа пользователя
			if (editDialogWindow.ShowDialog() == true)
			{
				// Получаем измененные данные
				string cyryllicModified = editDialogWindow.TextBoxEditCyryllic.Text;
				string latinModified = editDialogWindow.TextBoxEditLatin.Text;

				// Закрываем диалоговое окно
				editDialogWindow.Close();

				if (cyryllicModified.Length > 30 || latinModified.Length > 40)
				{
					ShowNotification("SnackBarNotAllowedLength");
					return;
				}

				string link = "http://translit.osmium.kz/api/word?";

				Dispatcher.InvokeAsync(async () =>
				{
					HttpClient client = new HttpClient();

					// Создаем параметры
					var values = new Dictionary<string, string>
					{
						{"token", User.Token},
						{"id", id.ToString() }
					};

					if (cyryllic != cyryllicModified)
					{
						values.Add("cyrl", cyryllicModified);
					}

					if (latin != latinModified)
					{
						values.Add("latn", latinModified);
					}

					var content = new FormUrlEncodedContent(values);

					// Выполняем запрос
					var response = await client.PutAsync(link, content);

					// Выполняем следующие действия взамисимости от ответа сервера
					if (response.StatusCode == HttpStatusCode.OK)
					{
						// Изменяем слово в локальной базе
						using (LiteDatabase db = new LiteDatabase(ConfigurationManager.ConnectionStrings["LiteDatabaseConnection"].ConnectionString))
						{
							var words = db.GetCollection<Word>("Words");
							var word = words.FindById(id);
							word.Cyryllic = cyryllicModified;
							word.Latin = latinModified;
							words.Update(word);
							UpdateDataGridWords();
						}
						await ShowAsyncNotification("SnackBarRecordEdited");
					}
					else if (response.StatusCode == HttpStatusCode.InternalServerError)
					{
						await ShowAsyncNotification("SnackBarServerSideError");
					}
					else if (response.StatusCode == HttpStatusCode.Conflict)
					{
						await ShowAsyncNotification("SnackBarTheWordIsAlreadyInTheDatabase");
					}
					else
					{
						await ShowAsyncNotification("SnackBarError");
					}
				});
			}
			else
			{
				ShowNotification("SnackBarEditingEntryCanceled");
			}
		}

		// Нажатие кнопки удаления слова
		private void ButtonDeleteWord_OnClick(object sender, RoutedEventArgs e)
		{
			// Получаем подтверждение пользователя
			DeleteDialogWindow deleteDialogWindow = new DeleteDialogWindow();

			if (deleteDialogWindow.ShowDialog() == true)
			{
				// Получаем Id выбранной
				int id = ((Word)((Button)sender).DataContext).Id;
				// Строим адрес
				string link = "http://translit.osmium.kz/api/word?token=" + User.Token + "&id=" + id;
				Dispatcher.InvokeAsync(async () =>
				{
					HttpClient client = new HttpClient();

					// Выполняем запрос
					var response = client.DeleteAsync(link).Result;

					// Выполняем следующие действия взамисимости от ответа сервера
					if (response.StatusCode == HttpStatusCode.OK)
					{
						using (LiteDatabase db = new LiteDatabase(ConfigurationManager.ConnectionStrings["LiteDatabaseConnection"].ConnectionString))
						{
							// Получаем коллекцию Words
							var words = db.GetCollection<Word>("Words");
							// Удаляем выбранный Id
							words.Delete(id);
							// Обновляем список слов
							UpdateDataGridWords();
							// Выводим уведомление
							await ShowAsyncNotification("SnackBarWordDeleted");
						}
					}
					else if (response.StatusCode == HttpStatusCode.InternalServerError)
					{
						await ShowAsyncNotification("SnackBarServerSideError");
					}
					else
					{
						await ShowAsyncNotification("SnackBarError");
					}
				});
			}
			else
			{
				ShowNotification("SnackBarDeleteCanceled");
			}
		}

		// Нажатие кнопки обновления списка
		private void ButtonUpdateDataGrid_OnClick(object sender, RoutedEventArgs e)
		{
			UpdateDataGridWords();
		}
	}
}