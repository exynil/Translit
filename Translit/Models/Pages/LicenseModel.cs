using System;
using System.IO;
using System.Text;
using System.Windows;

namespace Translit.Models.Pages
{
    internal class LicenseModel : ILicenseModel
    {
        public string ReadLicense()
        {
            var license = new Uri("resources/textdocuments/license.txt", UriKind.Relative);
            var licenseStream = Application.GetResourceStream(license)?.Stream;

            if (licenseStream == null) return "";

            using (var reader = new StreamReader(licenseStream, Encoding.Default))
            {
                return reader.ReadToEnd();
            }
        }
    }
}