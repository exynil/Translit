using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Translit.ViewModels.Pages
{
    internal class AboutViewModel : INotifyPropertyChanged
    {
        private string _copyright;
        private string _programName;
        private string _version;

        public AboutViewModel()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;

            ProgramName = "Translit";

            Copyright = $"© Kim Maxim, Osmium 2018-{DateTime.Now.Year}";

            Version = $"{GetRes("TextBlockVersion")} {version.Major}.{version.Minor} ({version.Build})";
        }

        public string ProgramName
        {
            get => _programName;
            set
            {
                _programName = value;
                OnPropertyChanged();
            }
        }

        public string Copyright
        {
            get => _copyright;
            set
            {
                _copyright = value;
                OnPropertyChanged();
            }
        }

        public string Version
        {
            get => _version;
            set
            {
                _version = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

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