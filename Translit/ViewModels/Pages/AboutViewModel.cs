using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Translit.ViewModels.Pages
{
	class AboutViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		public string ProgramName { get; set; }
		public string Copyright { get; set; }
		public string Version { get; set; }

		public AboutViewModel()
		{
			var version = Assembly.GetExecutingAssembly().GetName().Version;

			ProgramName = "Translit";

			Copyright = "© Kim Maxim, Osmium 2018-" + DateTime.Now.Year;

			Version = $"{GetRes("TextBlockVersion")} {version.Major}.{version.Minor} ({version.Build})";
		}

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public string GetRes(string key)
		{
			return Application.Current.Resources[key].ToString();
		}
	}
}
