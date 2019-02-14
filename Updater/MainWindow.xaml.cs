using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace Updater
{
    /// <summary>
    /// Логика Updater
    /// 1 - Удалить все файлы кроме [Файлов исключений]
    /// 2 - Удалить все папки кроме [Папок исключений]
    /// 3 - Распаковать архив с обновлением Translit.zip в текущую директорию
    /// 4 - Удалить архив с обновлением
    /// 5 - Запустить программу
    /// </summary>
    public partial class MainWindow
    {
        public string ArchiveWithUpdate = @".\Translit.zip";
        public string[] ExceptionalFiles = { "updater.exe", "localdb.db", "translit.zip" };
        public string[] ExceptionalFoldes = { "logs" };

        public MainWindow()
        {
            InitializeComponent();
            Update();
        }

        private void Update()
        {
            Task.Factory.StartNew(() =>
            {
                // Выбираем все файлы не входящий в список файлов исключений
                var files = new DirectoryInfo(Environment.CurrentDirectory).EnumerateFiles()
                    .Where(f => !ExceptionalFiles.Contains(f.Name.ToLower()))
                    .Select(f => f.FullName).ToArray();

                // Удаляем файлы
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

                // Выбираем все папки не входящий в список папок исключений
                var directories = new DirectoryInfo(Environment.CurrentDirectory)
                    .EnumerateDirectories()
                    .Where(d => !ExceptionalFoldes.Contains(d.Name.ToLower()))
                    .Select(d => d.FullName).ToArray();

                // Удаляем папки
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

                // Раскпаковываем программу
                using (var archive = ZipFile.OpenRead(ArchiveWithUpdate))
                {
                    archive.ExtractToDirectory(Environment.CurrentDirectory);
                }

                // Удаляем архив с обновлением
                File.Delete(ArchiveWithUpdate);

                // Запускаем Translit.exe
                Process.Start(@"Translit.exe");

                // Выходим из программы
                Close();
            });
        }
    }
}
