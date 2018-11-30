using LiteDB;
using System.Configuration;
using System.Windows;
using Translit.Entity;

namespace Translit.Pages
{
	public partial class SymbolsPage
	{
		public SymbolsPage()
		{
			InitializeComponent();
		}
		private void Page_Loaded(object sender, RoutedEventArgs e)
		{
			UpdateDataGridSymbols();
		}
		private void UpdateDataGridSymbols()
		{
			using (LiteDatabase db = new LiteDatabase(ConfigurationManager.ConnectionStrings["LiteDatabaseConnection"].ConnectionString))
			{
				DataGridSymbols.ItemsSource = db.GetCollection<Symbol>("Symbols").FindAll();
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
