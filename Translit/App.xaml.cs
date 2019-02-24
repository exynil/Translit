using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using Translit.Models.Other;
using Translit.Properties;

namespace Translit
{
    public partial class App
    {
        public App()
        {
            InitializeComponent();
        }

        public static CultureInfo Language
        {
            get => Thread.CurrentThread.CurrentUICulture;
            set
            {
                // Меняем язык приложения:
                Thread.CurrentThread.CurrentUICulture = value;

                // Создаём ResourceDictionary для новой культуры
                var dict = new ResourceDictionary
                {
                    Source = new Uri($"Resources/Languages/language.{value.Name}.xaml", UriKind.Relative)
                };

                Current.Resources.MergedDictionaries.Add(dict);
            }
        }

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            Analytics.SendUserData();
            Settings.Default.Save();
        }

        private void App_OnStartup(object sender, StartupEventArgs e)
        {

            Language = Settings.Default.Language;

            if (!File.Exists(@"Translit.zip")) return;

            try
            {
                Process.Start(@"Updater.exe");
                Environment.Exit(0);
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}