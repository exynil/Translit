using Translit.Models.Pages;
using Translit.Views.Pages;

namespace Translit.Presenters.Pages
{
	class LicensePresenter : ILicensePresenter
	{
		private ILicenseModel Model { get; }
		private ILicenseView View { get; }

		public LicensePresenter(ILicenseView view)
		{
			Model = new LicenseModel();
			View = view;
		}

		public void LoadLicense()
		{
			var text = Model.ReadLicense();
			View.SetLicense(text);
		}
	}
}
