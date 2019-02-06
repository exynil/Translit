using System;
using System.IO;
using System.Resources;
using System.Windows;

namespace Translit.Models.Pages
{
	class LicenseModel : ILicenseModel
	{
		public string ReadLicense()
		{
			var licenseStream = Application.GetResourceStream(new Uri("resources/txtfiles/license.txt", UriKind.Relative))?.Stream;
			using (var reader = new StreamReader(licenseStream ?? throw new InvalidOperationException()))
			{
				return reader.ReadToEnd();
			}
		}
	}
}
