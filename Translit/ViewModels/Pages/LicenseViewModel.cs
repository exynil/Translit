using System.ComponentModel;
using System.Runtime.CompilerServices;
using Translit.Models.Pages;

namespace Translit.ViewModels.Pages
{
	class LicenseViewModel : INotifyPropertyChanged
	{
		private string _license;
		public event PropertyChangedEventHandler PropertyChanged;
		public LicenseModel Model { get; set; }

		public string License
		{
			get => _license;
			set
			{
				_license = value;
				OnPropertyChanged();
			}
		}

		public LicenseViewModel()
		{
			Model = new LicenseModel();
			License = Model.ReadLicense();
		}

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
