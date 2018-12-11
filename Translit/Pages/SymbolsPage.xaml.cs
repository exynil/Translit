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
	}
}
