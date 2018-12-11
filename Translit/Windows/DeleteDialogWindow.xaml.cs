using System.Windows;

namespace Translit.Windows
{
	public partial class DeleteDialogWindow : Window
	{
		public DeleteDialogWindow()
		{
			InitializeComponent();
		}
		private void Accept_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
			Close();
		}
	}
}
