using System;
using System.Windows;

namespace Translit.Windows
{
	public partial class EditDialogWindow
	{
		public EditDialogWindow(string cyryllic, string latin)
		{
			InitializeComponent();
			TextBoxEditCyryllic.Text = cyryllic;
			TextBoxEditLatin.Text = latin;
		}
		private void Accept_Click(object sender, RoutedEventArgs e)
		{
			if (TextBoxEditCyryllic.Text != String.Empty && TextBoxEditLatin.Text != String.Empty)
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
