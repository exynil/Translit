using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using Newtonsoft.Json;
using Translit.Entity;
using Translit.Properties;

namespace Translit
{
	public partial class App
	{
		public static string[] Arguments { get; set; }

		private static List<CultureInfo> _languages = new List<CultureInfo>();

		public static List<CultureInfo> Languages => _languages;

		// Событие для оповещения всех окон приложения
		public static event EventHandler LanguageChanged;

		public App()
		{
			InitializeComponent();
			LanguageChanged += App_LanguageChanged;

			_languages.Clear();

			_languages.Add(new CultureInfo("ru-RU"));
			_languages.Add(new CultureInfo("kk-KZ"));
			_languages.Add(new CultureInfo("en-US")); //Нейтральная культура для этого проекта

			Language = Settings.Default.DefaultLanguage;
		}

		public static CultureInfo Language
		{
			get => Thread.CurrentThread.CurrentUICulture;
			set
			{
				if (value == null) throw new ArgumentNullException(nameof(value));
				if (value != Thread.CurrentThread.CurrentUICulture)
				{
					// 1. Меняем язык приложения:
					Thread.CurrentThread.CurrentUICulture = value;

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

					// 3. Находим старую ResourceDictionary и удаляем его и добавляем новую ResourceDictionary
					ResourceDictionary oldDict = (from d in Current.Resources.MergedDictionaries
						where d.Source != null && d.Source.OriginalString.StartsWith("Resources/Languages/language.")
						select d).First();
					if (oldDict != null)
					{
						int ind = Current.Resources.MergedDictionaries.IndexOf(oldDict);
						Current.Resources.MergedDictionaries.Remove(oldDict);
						Current.Resources.MergedDictionaries.Insert(ind, dict);
					}
					else
					{
						Current.Resources.MergedDictionaries.Add(dict);
					}

					// 4. Вызываем событие для оповещения всех окон.
					LanguageChanged?.Invoke(Current, new EventArgs());
				}
			}
		}

		private void App_LanguageChanged(Object sender, EventArgs e)
		{
			Settings.Default.DefaultLanguage = Language;
			GeneralSettings generalSettings = new GeneralSettings {Language = Language.CompareInfo.Name};
			File.WriteAllText(@".\Translit\External settings.json", JsonConvert.SerializeObject(generalSettings));
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
				Environment.Exit(0);
			}
			else if (Arguments[0] != "Cy9I*@dw0Zh_fj_KOPbI@QBS6Perfk%k#)5kGK0@XaQCY)@sj2Tex(Rh7bJK")
			{
				Environment.Exit(0);
			}
		}
	}
}
