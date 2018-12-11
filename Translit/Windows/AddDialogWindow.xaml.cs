using System;
using System.Windows;

namespace Translit.Windows
{
	public partial class AddDialogWindow
	{
		public AddDialogWindow()
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
				TextBlockMessage.Text = "Пожалуйста, заполните все поля";
			}
		}
	}
}
