using Translit.Models.Pages;
using Translit.Presenters.Pages;

namespace Translit.Views.Pages
{
	public partial class SettingsView
	{
		public SettingsPresenter Presenter { get; }
		public SettingsView()
		{
			InitializeComponent();
			Presenter = new SettingsPresenter(new SettingsModel(), this);
		}
	}
}
