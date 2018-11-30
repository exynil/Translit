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

		// Загрузка базы данных
		private void ButtonDownloadDatabase_Click(object sender, RoutedEventArgs e)
		{
			//Dispatcher.InvokeAsync(() =>
			//{
			//	string link = "http://translit.osmium.kz/api/word";
			//	HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(link);
			//	HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

			//	using (StreamReader stream = new StreamReader(resp.GetResponseStream(), Encoding.UTF8))
			//	{
			//		var temp = stream.ReadToEnd();
			//	}
			//});
		}
	}
}
