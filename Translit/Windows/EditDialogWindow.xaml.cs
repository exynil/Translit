using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
