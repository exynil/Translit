using Translit.Models.Pages;
using Translit.Views.Pages;

namespace Translit.Presenters.Pages
{
	public class SettingsPresenter
	{
		public SettingsModel Model { get; }
		public SettingsView View { get; }

		public SettingsPresenter(SettingsModel model, SettingsView view)
		{
			Model = model;
			View = view;
		}
	}
}
