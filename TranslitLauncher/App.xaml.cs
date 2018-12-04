using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Web.Script.Serialization;
using System.Windows;
using Newtonsoft.Json;
using TranslitLauncher.Properties;

namespace TranslitLauncher
{
	public partial class App
	{
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
			StreamReader streamReader = new StreamReader(@"Translit\External settings.json");
			var dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(streamReader.ReadToEnd());
			Language = new CultureInfo(dictionary["language"]);
			streamReader.Close();
		}
	}
}
