using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;
using Translit.Entity;
using Translit.Presenters.Pages;

namespace Translit.Views.Pages
{
	public partial class WordsView : IWordsView
	{
		private IWordsPresenter Presenter { get; }
		public WordsView()
		{
			InitializeComponent();
			Presenter = new WordsPresenter(this);
		}
		private void Page_Loaded(object sender, RoutedEventArgs e)
		{
			Presenter.OnPageLoaded();
			Loaded -= Page_Loaded;
		}
		public void UpdateWords(IEnumerable<Word> words)
		{
			Dispatcher.Invoke(() => { DataGridWords.ItemsSource = words; }, DispatcherPriority.Background);
		}
	}
}
