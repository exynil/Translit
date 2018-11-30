using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
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
		private readonly string _programApi;
		// Версия программы
		private string _programTagName;
		// Ссылка на скачивание программы
		private Uri _programUrl;

		// Название программы
		private readonly string _programFileName;
		// Название загружаемого файла
		private readonly string _downloadFileName;

		public MainWindow()
		{
			InitializeComponent();
			_programFileName = "Translit.exe";
			_downloadFileName = "Translit.download";
			_programApi = @"https://api.github.com/repos/OsmiumKZ/Translit/releases/latest";
		}

		private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
		{
			//Получение данных от api
			if (DownloadData())
			{
				// Проверка обновления
				if (CheckUpdate())
				{
					TextBlockMessage.Text = Application.Current.Resources["TextBlockNewVersionIsAvailableUpdateIsDownloading"].ToString();
					ProgressBarDownload.Visibility = Visibility.Visible;
					DownloadUpdate();
				}
				else
				{
					TextBlockMessage.Text = Application.Current.Resources["TextBlockRunTheProgram"].ToString();
					RunProgram();
					//Close();
				}
			}
			else
			{
				TextBlockMessage.Text = Application.Current.Resources["TextBlockRunTheProgram"].ToString();
				RunProgram();
				//Close();
			}
		}

		// Запуск программы
		private void RunProgram()
		{
			try
			{
				// Пытаемся запустить программу
				Process.Start(_programFileName, "Cy9I*@dw0Zh_fj_KOPbI@QBS6Perfk%k#)5kGK0@XaQCY)@sj2Tex(Rh7bJK");
			}
			catch (Exception ex)
			{
				TextBlockMessage.Text = Application.Current.Resources["TextBlockAnErrorHasOccurred"] + ex.Message;
			}
		}

		private bool DownloadData()
		{
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11;

			WebClient client = new WebClient();

			client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

			try
			{
				Stream stream = client.OpenRead(_programApi);
				StreamReader streamReader = new StreamReader(stream ?? throw new InvalidOperationException());

				var data = JsonConvert.DeserializeObject<GithubApi>(streamReader.ReadToEnd());

				_programTagName = data.TagName;
				_programUrl = data.Assets[0].BrowserDownloadUrl;

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
			int currentVersion = int.Parse(Settings.Default.Version.Substring(1).Replace(".", ""));
			int newVersion = int.Parse(_programTagName.Substring(1).Replace(".", ""));

			if (currentVersion < newVersion)
			{
				return true;
			}
			return false;
		}

		private void DownloadUpdate()
		{
			try
			{
				ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
				WebClient webClient = new WebClient();
				webClient.DownloadFileCompleted += Completed;
				webClient.DownloadProgressChanged += ProgressChanged;
				webClient.DownloadFileAsync(_programUrl, _downloadFileName);
			}
			catch (Exception e)
			{
				Error(e.Message + " " + _downloadFileName);
			}
		}

		private void Completed(object sender, AsyncCompletedEventArgs e)
		{
			// Заменяем новый скачанный файл на старый
			File.Replace(_downloadFileName, _programFileName, null);
			// Выводим уведомление о запуске программы
			TextBlockMessage.Text = Application.Current.Resources["TextBlockRunTheProgram"].ToString();
			// Меняем версию программы в настрйоках
			Settings.Default.Version = _programTagName;
			// Запускаем программу
			RunProgram();
			// Закрываем лаунчер
			Close();
		}

		// Собитие на изменение процента загрузки
		private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
		{
			// Изменяем значение ProgressBar
			ProgressBarDownload.Value = e.ProgressPercentage;
		}

		// Вывод информации об ошибке
		private void Error(string message)
		{
			// Выводим информацию об ошибке
			TextBlockErrorMessage.Text = message;
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
			DoubleAnimation aRotateAnimation = new DoubleAnimation
			{
				From = 0,
				To = 45,
				Duration = TimeSpan.FromSeconds(1.5),
				EasingFunction = new ElasticEase { EasingMode = EasingMode.EaseOut, Oscillations = 5 }
			};

			DoubleAnimation bRotateAnimation = new DoubleAnimation
			{
				From = 45,
				To = 0,
				Duration = TimeSpan.FromSeconds(1.5),
				EasingFunction = new ElasticEase { EasingMode = EasingMode.EaseOut, Oscillations = 5 }
			};

			DoubleAnimation cRotateAnimation = new DoubleAnimation
			{
				From = 0,
				To = -45,
				Duration = TimeSpan.FromSeconds(1.5),
				EasingFunction = new ElasticEase { EasingMode = EasingMode.EaseOut, Oscillations = 5 }
			};
			DoubleAnimation dRotateAnimation = new DoubleAnimation
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