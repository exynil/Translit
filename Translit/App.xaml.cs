using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using Translit.Properties;

namespace Translit
{
	public partial class App
	{
		public static string[] Arguments { get; set; }

		public App()
		{
			InitializeComponent();

			Language = Settings.Default.DefaultLanguage;
		}

		public static CultureInfo Language
		{
			get => Thread.CurrentThread.CurrentUICulture;
			set
			{
				// 1. Меняем язык приложения:
				Thread.CurrentThread.CurrentUICulture = value;

				//2.Создаём ResourceDictionary для новой культуры
				var dict = new ResourceDictionary
				{
					Source = new Uri($"Resources/Languages/language.{value.Name}.xaml", UriKind.Relative)
				};

				Current.Resources.MergedDictionaries.Add(dict);
			}
		}

		private void App_OnExit(object sender, ExitEventArgs e)
		{
			Settings.Default.Save();
		}

		private void App_OnStartup(object sender, StartupEventArgs e)
		{
			Arguments = new string[e.Args.Length];
			for (var i = 0; i < e.Args.Length; i++)
			{
				Arguments[i] = e.Args[i];
			}
#if !DEBUG
			if (Arguments.Length == 0)
			{
				Environment.Exit(0);
			}
			else if (Arguments[0] != "Cy9I*@dw0Zh_fj_KOPbI@QBS6Perfk%k#)5kGK0@XaQCY)@sj2Tex(Rh7bJK")
			{
				Environment.Exit(0);
			}
#endif
		}
	}
}