using System;
using System.Globalization;
using System.Windows;
using TranslitLauncher.Properties;

namespace TranslitLauncher
{
	public partial class App
	{
		public static string[] Arguments { get; set; }
		public App()
		{
			InitializeComponent();
		}

		public static CultureInfo Language
		{
			get => System.Threading.Thread.CurrentThread.CurrentUICulture;
			set
			{
				if (value == null) throw new ArgumentNullException(nameof(value));
				if (value != System.Threading.Thread.CurrentThread.CurrentUICulture)
				{
					// 1. Меняем язык приложения:
					System.Threading.Thread.CurrentThread.CurrentUICulture = value;

					// 2. Создаём ResourceDictionary для новой культуры
					ResourceDictionary dict = new ResourceDictionary();
					switch (value.Name)
					{
						case "ru-RU":
							dict.Source = new Uri($"Resources/Languages/language.{value.Name}.xaml", UriKind.Relative);
							break;
						case "kk-KZ":
							dict.Source = new Uri($"Resources/Languages/language.{value.Name}.xaml", UriKind.Relative);
							break;
						default:
							dict.Source = new Uri("Resources/Languages/language.xaml", UriKind.Relative);
							break;
					}

					Current.Resources.MergedDictionaries.Add(dict);
				}
			}
		}

		private void App_OnExit(object sender, ExitEventArgs e)
		{
			Settings.Default.Save();
		}

		private void App_OnStartup(object sender, StartupEventArgs e)
		{
			Arguments = new string[e.Args.Length];
			for (int i = 0; i < e.Args.Length; i++)
			{
				Arguments[i] = e.Args[i];
			}

			if (Arguments.Length == 0)
			{
				Language = new CultureInfo(Settings.Default.DefaultLanguage);
			}
			else if (Arguments[0] == "ru-RU" || Arguments[0] == "en-US" || Arguments[0] == "kk-KZ")
			{
				
				Settings.Default.DefaultLanguage = Arguments[0];
				Settings.Default.Save();
				Environment.Exit(0);
			}
		}
	}
}
