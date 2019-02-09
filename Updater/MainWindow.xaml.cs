using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows;

namespace Updater
{
	public partial class MainWindow
	{
		public string ArchiveWithUpdate { get; set; }
		public MainWindow()
		{
			InitializeComponent();
			ArchiveWithUpdate = @"Update\Translit.zip";
		}

		private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
		{
			var files = new DirectoryInfo(Environment.CurrentDirectory).EnumerateFiles()
				.Where(f => f.Name != "Updater.exe" && f.Name != ArchiveWithUpdate)
				.Select(f => f.FullName).ToArray();

			foreach (var f in files)
			{
				try
				{
					File.Delete(f);
				}
				catch (Exception)
				{
					// ignored
				}
			}

			using (var archive = ZipFile.OpenRead(ArchiveWithUpdate))
			{
				archive.ExtractToDirectory(Environment.CurrentDirectory);
			}

			File.Delete(ArchiveWithUpdate);

			Process.Start(@"Translit.exe", "Update installed");
			Close();
		}
	}
}
