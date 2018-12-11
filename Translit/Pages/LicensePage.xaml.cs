using System.IO;
using System.Windows.Controls;

namespace Translit.Pages
{
	public partial class LicensePage : Page
	{
		public LicensePage()
		{
			InitializeComponent();
			if (File.Exists(@"license.txt"))
			{
				using (StreamReader reader = new StreamReader(@"license.txt"))
				{
					RichTextBoxLicense.AppendText(reader.ReadToEnd());
				}
			}
		}
	}
}
