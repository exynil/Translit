using LiteDB;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Translit.Entity;
using Translit.Windows;

namespace Translit.Pages
{
	public partial class SymbolsEditorPage
	{
		public User User;
		private readonly Snackbar _snackbar;

		public SymbolsEditorPage(Snackbar snackbar)
		{
			InitializeComponent();
			_snackbar = snackbar;
		}

		private void Page_Loaded(object sender, RoutedEventArgs e)
		{
			UpdateDataGridSymbols();
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

		// Обновление списка
		private void UpdateDataGridSymbols()
		{
			using (LiteDatabase db = new LiteDatabase(ConfigurationManager.ConnectionStrings["LiteDatabaseConnection"].ConnectionString))
			{
				DataGridSymbols.ItemsSource = db.GetCollection<Symbol>("Symbols").FindAll();
			}
		}

		// Нажатие кнопки добавления нового слова
		private void ButtonAddSymbol_OnClick(object sender, RoutedEventArgs e)
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

				if (cyryllic.Length > 5 || latin.Length > 5)
				{
					ShowNotification("SnackBarNotAllowedLength");
					return;
				}

				string link = "http://translit.osmium.kz/api/symbol?";

				HttpClient client = new HttpClient();

				// Создаем параметры
				var content = new FormUrlEncodedContent(new Dictionary<string, string>
				{
					{"token", User.Token},
					{"cyrl", cyryllic },
					{"latn", latin}
				});

				Dispatcher.InvokeAsync(async () =>
				{
					// Выполняем запрос
					var response = await client.PostAsync(link, content);

					// Полученный ответ десериализуем в объект Word
					Symbol addedSymbol = JsonConvert.DeserializeObject<Symbol>(await response.Content.ReadAsStringAsync());

					// Выполняем следующие действия взамисимости от ответа сервера
					if (response.StatusCode == HttpStatusCode.Created)
					{
						// Добавляем слово в локальную базу
						using (LiteDatabase db = new LiteDatabase(ConfigurationManager.ConnectionStrings["LiteDatabaseConnection"].ConnectionString))
						{
							var symbols = db.GetCollection<Symbol>("Symbols");
							symbols.Insert(addedSymbol);
							UpdateDataGridSymbols();
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
		private void ButtonEditSymbol_OnClick(object sender, RoutedEventArgs e)
		{
			int id = ((Symbol)((Button)sender).DataContext).Id;
			string cyryllic = ((Symbol)((Button)sender).DataContext).Cyryllic;
			string latin = ((Symbol)((Button)sender).DataContext).Latin;
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

				if (cyryllicModified.Length > 5 || latinModified.Length > 5)
				{
					ShowNotification("SnackBarNotAllowedLength");
					return;
				}

				string link = "http://translit.osmium.kz/api/symbol?";

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
							var symbols = db.GetCollection<Symbol>("Symbols");
							var symbol = symbols.FindById(id);
							symbol.Cyryllic = cyryllicModified;
							symbol.Latin = latinModified;
							symbols.Update(symbol);
							UpdateDataGridSymbols();
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
						Debug.WriteLine(response.StatusCode);
						await ShowAsyncNotification("SnackBarError");
					}
				});
			}
			else
			{
				ShowNotification("SnackBarRecordEditingCanceled");
			}
		}

		// Нажатие кнопки удаления слова
		private void ButtonDeleteSymbol_OnClick(object sender, RoutedEventArgs e)
		{
			// Получаем подтверждение пользователя
			DeleteDialogWindow deleteDialogWindow = new DeleteDialogWindow();

			if (deleteDialogWindow.ShowDialog() == true)
			{
				// Получаем Id выбранной
				int id = ((Symbol)((Button)sender).DataContext).Id;
				// Строим адрес
				string link = "http://translit.osmium.kz/api/symbol?token=" + User.Token + "&id=" + id;
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
							var symbols = db.GetCollection<Symbol>("Symbols");
							// Удаляем выбранный Id
							symbols.Delete(id);
							// Обновляем список слов
							UpdateDataGridSymbols();
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
			UpdateDataGridSymbols();
		}
	}
}