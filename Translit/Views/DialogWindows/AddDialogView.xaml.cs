﻿using System;
using System.Windows;

namespace Translit.Views.DialogWindows
{
	public partial class AddDialogView
	{
		public AddDialogView()
		{
			InitializeComponent();
		}
		private void Accept_Click(object sender, RoutedEventArgs e)
		{
			if (TextBoxAddCyryllic.Text != String.Empty && TextBoxAddLatin.Text != String.Empty)
			{
				DialogResult = true;
			}
			else
			{
				TextBlockMessage.Text = GetRes("TextBlockPleaseFillInAllFields");
			}
		}

		// Получение ресурса по ключу
		public string GetRes(string key)
		{
			return Application.Current.Resources[key].ToString();
		}
	}
}