using LiteDB;
using System.Configuration;
using System.Windows;
using Translit.Entity;

namespace Translit.Pages
{
	public partial class WordsPage
	{
		public WordsPage()
		{
			InitializeComponent();
		}
		private void Page_Loaded(object sender, RoutedEventArgs e)
		{
			UpdateDataGridWords();
		}
		private void UpdateDataGridWords()
		{
			using (LiteDatabase db = new LiteDatabase(ConfigurationManager.ConnectionStrings["LiteDatabaseConnection"].ConnectionString))
			{
				DataGridWords.ItemsSource = db.GetCollection<Word>("Words").FindAll();
			}
		}
	}
}
