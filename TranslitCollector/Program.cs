﻿using System;
using System.Diagnostics;
using System.IO;

namespace TranslitCollector
{
	class Program
	{
		static void Main()
		{
#if DEBUG
			var translitLauncher = @"..\..\..\TranslitLauncher\bin\Debug";
			var translit = @"..\..\..\Translit\bin\Debug";
#else
			var translitLauncher = @"..\..\..\TranslitLauncher\bin\Release";
			var translit = @"..\..\..\Translit\bin\Release";
#endif
			try
			{
				Directory.Delete(@"Translit", true);
			}
			catch (Exception)
			{
				// ignored
			}
			
			DirectoryCopy(translitLauncher, @".\Translit", true);
			DirectoryCopy(translit, @".\Translit\Translit", true);
			Process.Start("explorer.exe", @".\Translit");
		}
		private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
		{
			// Get the subdirectories for the specified directory.
			DirectoryInfo dir = new DirectoryInfo(sourceDirName);

			if (!dir.Exists)
			{
				throw new DirectoryNotFoundException(
					"Source directory does not exist or could not be found: "
					+ sourceDirName);
			}

			DirectoryInfo[] dirs = dir.GetDirectories();
			// If the destination directory doesn't exist, create it.
			if (!Directory.Exists(destDirName))
			{
				Directory.CreateDirectory(destDirName);
			}

			// Get the files in the directory and copy them to the new location.
			FileInfo[] files = dir.GetFiles();
			foreach (FileInfo file in files)
			{
				string temppath = Path.Combine(destDirName, file.Name);
				file.CopyTo(temppath, false);
			}

			// If copying subdirectories, copy them and their contents to new location.
			if (copySubDirs)
			{
				foreach (DirectoryInfo subdir in dirs)
				{
					string temppath = Path.Combine(destDirName, subdir.Name);
					DirectoryCopy(subdir.FullName, temppath, true);
				}
			}
		}
	}
}
