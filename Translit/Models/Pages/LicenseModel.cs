using System;
using System.IO;
using System.Windows;

namespace Translit.Models.Pages
{
    internal class LicenseModel : ILicenseModel
    {
        public string ReadLicense()
        {
            var license = new Uri("resources/txtdocuments/license.txt", UriKind.Relative);
            var licenseStream = Application.GetResourceStream(license)?.Stream;

            if (licenseStream == null) return "";

            using (var reader = new StreamReader(licenseStream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}