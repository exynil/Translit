﻿using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using MaterialDesignThemes.Wpf;
using Translit.Entity;
using Translit.Presenters.Pages;
using Translit.Views.Windows;
using Translit.Windows;

namespace Translit.Views.Pages
{
	public partial class SymbolsEditorView : ISymbolsEditorView
	{
		private ISymbolsEditorPresenter Presenter { get; }
		public Snackbar SnackbarNotification { get; set; }

		public SymbolsEditorView()
		{
			InitializeComponent();
			Presenter = new SymbolsEditorPresenter(this);
		}

		private void Page_Loaded(object sender, RoutedEventArgs e)
		{
			Presenter.OnPageLoaded();
			// Подключаем внешний уведомитель
			SnackbarNotification = ((MainWindowView)Window.GetWindow(this))?.SnackbarNotification;
			Loaded -= Page_Loaded;
		}

		// Обновление списка символов
		public void UpdateSymbols(IEnumerable<Symbol> symbols)
		{
			DataGridSymbols.Dispatcher.Invoke(() => { DataGridSymbols.ItemsSource = symbols; }, DispatcherPriority.Background);
		}

		// Показ уведомления по ключу из ресурсов
		public void ShowNotification(string key)
		{
			SnackbarNotification.Dispatcher.Invoke(() => { SnackbarNotification.MessageQueue.Enqueue(GetRes(key)); },
				DispatcherPriority.Background);
		}

		// Нажатие кнопки добавления нового слова
		private void ButtonAddSymbol_OnClick(object sender, RoutedEventArgs e)
		{
			// Создаем диалоговое окно
			var addDialogWindow = new AddDialogWindow();

			// Ожидаем ответ пользователя
			if (addDialogWindow.ShowDialog() == true)
			{
				var cyryllic = addDialogWindow.TextBoxAddCyryllic.Text;
				var latin = addDialogWindow.TextBoxAddLatin.Text;

				// Закрываем диалоговое окно
				addDialogWindow.Close();

				Presenter.AddNewSymbol(cyryllic, latin);
			}
			else
			{
				ShowNotification("SnackBarAddingEntryCanceled");
			}
		}

		// Нажатие кнопки изменения слова
		private void ButtonEditSymbol_OnClick(object sender, RoutedEventArgs e)
		{
			var id = ((Symbol)((Button)sender).DataContext).Id;
			var cyryllic = ((Symbol)((Button)sender).DataContext).Cyryllic;
			var latin = ((Symbol)((Button)sender).DataContext).Latin;

			// Создаем диалоговое окно
			var editDialogWindow = new EditDialogWindow(cyryllic, latin);

			// Ожидаем ответа пользователя
			if (editDialogWindow.ShowDialog() == true)
			{
				// Получаем измененные данные
				var cyryllicModified = editDialogWindow.TextBoxEditCyryllic.Text;
				var latinModified = editDialogWindow.TextBoxEditLatin.Text;

				// Закрываем диалоговое окно
				editDialogWindow.Close();

				cyryllic = cyryllic != cyryllicModified ? cyryllicModified : null;

				latin = latin != latinModified ? latinModified : null;

				Presenter.EditSymbol(id, cyryllic, latin);
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
			var deleteDialogWindow = new DeleteDialogWindow();

			if (deleteDialogWindow.ShowDialog() == true)
			{
				// Получаем Id выбранной
				var id = ((Symbol)((Button)sender).DataContext).Id;

				Presenter.DeleteSymbol(id);
			}
			else
			{
				ShowNotification("SnackBarDeleteCanceled");
			}
		}

		// Нажатие кнопки обновления списка
		private void ButtonUpdateDataGrid_OnClick(object sender, RoutedEventArgs e)
		{
			Presenter.UpdateSymbols();
		}

		// Получение ресурса по ключу
		public string GetRes(string key)
		{
			return Application.Current.Resources[key].ToString();
		}
	}
}