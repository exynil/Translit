using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;
using Translit.Entity;
using Translit.Models.Pages;
using Translit.Presenters.Pages;

namespace Translit.Views.Pages
{
	public partial class SymbolsView
	{
		public SymbolsPresenter Presenter { get; set; }
		public SymbolsView()
		{
			InitializeComponent();
			Presenter = new SymbolsPresenter(new SymbolsModel(), this);
		}

		private void Page_Loaded(object sender, RoutedEventArgs e)
		{
			Presenter.OnPageLoaded();
		}

		// Обновление списка символов
		public void UpdateSymbols(IEnumerable<Symbol> symbols)
		{
			Dispatcher.Invoke(() => { DataGridSymbols.ItemsSource = symbols; }, DispatcherPriority.Background);
		}
	}
}
