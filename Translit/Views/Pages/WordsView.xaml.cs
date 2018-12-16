using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;
using Translit.Entity;
using Translit.Models.Pages;
using Translit.Presenters.Pages;

namespace Translit.Views.Pages
{
	public partial class WordsView
	{
		public WordsPresenter Presenter { get; }
		public WordsView()
		{
			InitializeComponent();
			Presenter = new WordsPresenter(new WordsModel(), this);
		}
		private void Page_Loaded(object sender, RoutedEventArgs e)
		{
			Presenter.OnPageLoaded();
		}
		public void UpdateWords(IEnumerable<Word> words)
		{
			Dispatcher.Invoke(() => { DataGridWords.ItemsSource = words; }, DispatcherPriority.Background);
		}
	}
}
