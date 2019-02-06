using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using TranslitLauncher.Entity;
using TranslitLauncher.Properties;

namespace TranslitLauncher
{
	public partial class MainWindow
	{
		// Ссылка на api
		public string Api { get; set; }
		// Версия программы
		private string TagName { get; set; }
		// Ссылка на скачивание программы
		private Uri BrowserDownloadUrl { get; set; }
		// Название загружаемого файла
		public string DownloadZipArchiveName { get; set; }

		public MainWindow()
		{
			InitializeComponent();
			DownloadZipArchiveName = "Translit.zip";
			Api = @"https://api.github.com/repos/OsmiumKZ/Translit/releases/latest";
		}

		private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
		{
			//Получение данных от api
			if (DownloadData())
			{
				// Проверка обновления
				if (CheckUpdate())
				{
					StackPanelDownloadInfo.Visibility = Visibility.Visible;
					TextBlockDownloadMessage.Text = Application.Current.Resources["TextBlockNewVersionIsAvailableUpdateIsDownloading"].ToString();
					ProgressBarDownload.Visibility = Visibility.Visible;
					DownloadUpdate();
				}
				else
				{
					TextBlockInfo.Text = Application.Current.Resources["TextBlockRunTheProgram"].ToString();
					RunProgram();
					Close();
				}
			}
			else
			{
				TextBlockInfo.Text = Application.Current.Resources["TextBlockRunTheProgram"].ToString();
				RunProgram();
				Close();
			}
		}

		// Запуск программы
		private void RunProgram()
		{
			// Пытаемся запустить программу
			try
			{
				var startInfo = new ProcessStartInfo(@"Translit.exe", "Cy9I*@dw0Zh_fj_KOPbI@QBS6Perfk%k#)5kGK0@XaQCY)@sj2Tex(Rh7bJK")
				{
					WorkingDirectory = Directory.GetCurrentDirectory() + @"\Translit"
				};

				Process.Start(startInfo);
			}
			catch (Exception ex)
			{
				TextBlockInfo.Text = Application.Current.Resources["TextBlockAnErrorHasOccurred"] + ": " + ex.Message;
			}
		}

		private bool DownloadData()
		{
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11;

			var client = new WebClient();

			client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

			try
			{
				var stream = client.OpenRead(Api);
				var streamReader = new StreamReader(stream ?? throw new InvalidOperationException());

				var data = JsonConvert.DeserializeObject<GithubApi>(streamReader.ReadToEnd());

				TagName = data.TagName;
				BrowserDownloadUrl = data.Assets[0].BrowserDownloadUrl;

				stream.Close();
				streamReader.Close();
			}
			catch (Exception)
			{
				return false;
			}

			return true;
		}

		private bool CheckUpdate()
		{
			var currentVersion = int.Parse(Settings.Default.Version.Substring(1).Replace(".", ""));
			var newVersion = int.Parse(TagName.Substring(1).Replace(".", ""));

			return currentVersion < newVersion;
		}

		private void DownloadUpdate()
		{
			try
			{
				ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
				var webClient = new WebClient();
				webClient.DownloadFileCompleted += Completed;
				webClient.DownloadProgressChanged += ProgressChanged;
				webClient.DownloadFileAsync(BrowserDownloadUrl, DownloadZipArchiveName);
			}
			catch (Exception e)
			{
				Error($"{e.Message} {DownloadZipArchiveName}");
			}
		}

		private void Completed(object sender, AsyncCompletedEventArgs e)
		{
			StackPanelInstallInfo.Visibility = Visibility.Visible;
			TextBlockInstallMessage.Text = Application.Current.Resources["TextBlockProgramInstallation"].ToString();
			// Удаляем старую версию
			Directory.Delete("Translit", true);
			// Распаковываем скачанный архив
			using (ZipArchive archive = ZipFile.OpenRead(DownloadZipArchiveName))
			{
				archive.ExtractToDirectory(@".\");
			}
			File.Delete("Translit.zip");
			StackPanelDownloadInfo.Visibility = Visibility.Hidden;
			StackPanelInstallInfo.Visibility = Visibility.Hidden;
			// Выводим уведомление о запуске программы
			TextBlockInfo.Text = Application.Current.Resources["TextBlockRunTheProgram"].ToString();
			// Меняем версию программы в настрйоках
			Settings.Default.Version = TagName;
			// Запускаем программу
			RunProgram();
			// Закрываем лаунчер
			Close();
		}

		// Событие на изменение процента загрузки
		private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
		{
			// Изменяем значение ProgressBar
			ProgressBarDownload.Value = e.ProgressPercentage;
		}

		// Вывод информации об ошибке
		private void Error(string message)
		{
			// Выводим информацию об ошибке
			TextBlockInfo.Text = message;
		}

		// Закрытие лаунчера
		private void ButtonCloseWindow_OnClick(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			DragMove();
		}

		private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			TextBlockProgramName.Text = "Osmium";

			var aRotateAnimation = new DoubleAnimation
			{
				From = 0,
				To = 45,
				Duration = TimeSpan.FromSeconds(1.5),
				EasingFunction = new ElasticEase { EasingMode = EasingMode.EaseOut, Oscillations = 5 }
			};

			var bRotateAnimation = new DoubleAnimation
			{
				From = 45,
				To = 0,
				Duration = TimeSpan.FromSeconds(1.5),
				EasingFunction = new ElasticEase { EasingMode = EasingMode.EaseOut, Oscillations = 5 }
			};

			var cRotateAnimation = new DoubleAnimation
			{
				From = 0,
				To = -45,
				Duration = TimeSpan.FromSeconds(1.5),
				EasingFunction = new ElasticEase { EasingMode = EasingMode.EaseOut, Oscillations = 5 }
			};

			var dRotateAnimation = new DoubleAnimation
			{
				From = -45,
				To = -0,
				Duration = TimeSpan.FromSeconds(1.5),
				EasingFunction = new ElasticEase { EasingMode = EasingMode.EaseOut, Oscillations = 5 }
			};

			((TextBlock) sender).RenderTransform = new RotateTransform();
			((TextBlock)sender).RenderTransformOrigin = new Point(0.5, 0.5);

			((TextBlock)sender).RenderTransform.BeginAnimation(RotateTransform.AngleProperty, aRotateAnimation);
			((TextBlock)sender).RenderTransform.BeginAnimation(RotateTransform.AngleProperty, bRotateAnimation, HandoffBehavior.Compose);
			((TextBlock)sender).RenderTransform.BeginAnimation(RotateTransform.AngleProperty, cRotateAnimation, HandoffBehavior.Compose);
			((TextBlock)sender).RenderTransform.BeginAnimation(RotateTransform.AngleProperty, dRotateAnimation, HandoffBehavior.Compose);
		}
	}
}