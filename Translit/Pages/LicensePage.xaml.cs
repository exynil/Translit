using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
