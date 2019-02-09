﻿using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
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
			Arguments = e.Args.ToArray();

			if (Arguments.Length > 0)
			{
				if (Arguments[0] == "Update installed")
				{
					Settings.Default.UpdateReady = false;
				}
			}

			if (!Settings.Default.UpdateReady) return;

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