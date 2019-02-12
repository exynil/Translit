using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows;

namespace Updater
{
    /// <summary>
    /// Логика Updater
    /// 1 - Удалить все файлы кроме [Файлов исключений]
    /// 2 - Распаковать архив с обновлением Translit.zip в текущую директорию
    /// 3 - Удалить архив с обновлением
    /// 4 - Запустить программу
    /// </summary>
    public partial class MainWindow
    {
        public string ArchiveWithUpdate { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            ArchiveWithUpdate = @".\Translit.zip";
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            string[] exceptionalFiles = {"updater.exe", "localdb.db", "translit.zip"};

            // Выбираем все файлы не включая исключительные
            var files = new DirectoryInfo(Environment.CurrentDirectory).EnumerateFiles()
                .Where(f => !exceptionalFiles.Contains(f.Name.ToLower()))
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

            // Выбираем все папки
            var directories = new DirectoryInfo(Environment.CurrentDirectory)
                .EnumerateDirectories()
                .Select(d => d.FullName).ToArray();

            foreach (var d in directories)
            {
                try
                {
                    Directory.Delete(d, true);
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
