using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Translit.Models.Other;

namespace Translit.Views.DialogWindows
{
    public partial class LicenseDialogView : INotifyPropertyChanged
    {
        private string _license;

        public string License
        {
            get => _license;
            set
            {
                _license = value;
                OnPropertyChanged();
            }
        }

        public LicenseDialogView()
        {
            InitializeComponent();
            DataContext = this;
            License = ReadLicense();
        }

        public string ReadLicense()
        {
            var license = new Uri("resources/txtdocuments/license.txt", UriKind.Relative);
            var licenseStream = Application.GetResourceStream(license)?.Stream;

            if (licenseStream == null) return "";

            using (var reader = new StreamReader(licenseStream, Encoding.Default))
            {
                return reader.ReadToEnd();
            }
        }

        public ICommand Accept
        {
            get
            {
                return new DelegateCommand(o =>
                {
                    DialogResult = true;
                });
            }
        }

        public ICommand Reject
        {
            get
            {
                return new DelegateCommand(o =>
                {
                    DialogResult = false;
                });
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
