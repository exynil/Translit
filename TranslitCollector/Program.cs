using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace TranslitCollector
{
    class Program
	{
		static void Main()
		{
#if DEBUG
			const string updater = @"..\..\..\Updater\bin\Debug\Updater.exe";
			const string translit = @"..\..\..\Translit\bin\Debug";
#else
			var updater = @"..\..\..\Updater\bin\Release\Updater.exe";
			var translit = @"..\..\..\Translit\bin\Release";
#endif
			if (Directory.Exists("Translit"))
			{
				Directory.Delete(@"Translit", true);
			}

			DirectoryCopy(translit, @".\Translit", true);
			File.Copy(updater, @".\Translit\Updater.exe", true);
		    Sort();
			Process.Start("explorer.exe", @".\Translit");
		}

	    private static void Sort()
	    {
	        var files = new DirectoryInfo(@".\Translit").EnumerateFiles()
	            .Where(f => f.Extension == ".xml" || f.Extension == ".pdb").ToArray();

	        foreach (var f in files)
	        {
	            File.Delete(f.FullName);
	        }

	       files = new DirectoryInfo(@".\Translit").EnumerateFiles()
	            .Where(f => f.Extension == ".dll").ToArray();

	        Directory.CreateDirectory(@"Translit\Libraries");

	        foreach (var f in files)
	        {
                File.Move(f.FullName, $@"Translit\Libraries\{f.Name}");
	        }
        }

	    private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
		{
			// Get the subdirectories for the specified directory.
			var dir = new DirectoryInfo(sourceDirName);

			if (!dir.Exists)
			{
				throw new DirectoryNotFoundException(
					"Source directory does not exist or could not be found: "
					+ sourceDirName);
			}

			var dirs = dir.GetDirectories();
			// If the destination directory doesn't exist, create it.
			if (!Directory.Exists(destDirName))
			{
				Directory.CreateDirectory(destDirName);
			}

			// Get the files in the directory and copy them to the new location.
			var files = dir.GetFiles();
			foreach (var file in files)
			{
				var temppath = Path.Combine(destDirName, file.Name);
				file.CopyTo(temppath, false);
			}

			// If copying subdirectories, copy them and their contents to new location.
			if (!copySubDirs) return;
			{
				foreach (var subdir in dirs)
				{
					var temppath = Path.Combine(destDirName, subdir.Name);
					DirectoryCopy(subdir.FullName, temppath, true);
				}
			}
		}
	}
}
