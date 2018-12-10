using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Translit.Pages
{
	public partial class AboutPage
	{
		private readonly Frame _frameMain;
		private readonly Page _licensePage;
		
		public AboutPage(Frame frame)
		{
			InitializeComponent();
			_frameMain = frame;
			_licensePage = new LicensePage();
		}

		private void ButtonLicense_OnClick(object sender, RoutedEventArgs e)
		{
			_frameMain.NavigationService.Navigate(_licensePage);
		}

		private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
		{
			Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
			e.Handled = true;
		}

		private void Page_Loaded(object sender, RoutedEventArgs e)
		{
			TextBlockCopyright.Text = "© " + DateTime.Now.Year + " Osmium";
		}

		private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			int degrees = 20;
			DoubleAnimation aRotateAnimation = new DoubleAnimation
			{
				From = 0,
				To = degrees,
				Duration = TimeSpan.FromSeconds(1),
				EasingFunction = new ElasticEase { EasingMode = EasingMode.EaseOut, Oscillations = 5 }
			};

			DoubleAnimation bRotateAnimation = new DoubleAnimation
			{
				From = degrees,
				To = 0,
				Duration = TimeSpan.FromSeconds(1),
				EasingFunction = new ElasticEase { EasingMode = EasingMode.EaseOut, Oscillations = 5 }
			};

			DoubleAnimation cRotateAnimation = new DoubleAnimation
			{
				From = 0,
				To = -degrees,
				Duration = TimeSpan.FromSeconds(1),
				EasingFunction = new ElasticEase { EasingMode = EasingMode.EaseOut, Oscillations = 5 }
			};
			DoubleAnimation dRotateAnimation = new DoubleAnimation
			{
				From = -degrees,
				To = -0,
				Duration = TimeSpan.FromSeconds(1),
				EasingFunction = new ElasticEase { EasingMode = EasingMode.EaseOut, Oscillations = 5 }
			};

			((TextBlock)sender).RenderTransform = new RotateTransform();
			((TextBlock)sender).RenderTransformOrigin = new Point(0.5, 0.5);

			((TextBlock)sender).Dispatcher.Invoke(
				() => ((TextBlock)sender).RenderTransform.BeginAnimation(RotateTransform.AngleProperty, aRotateAnimation),
				DispatcherPriority.Background);
			((TextBlock)sender).Dispatcher.Invoke(
				() => ((TextBlock)sender).RenderTransform.BeginAnimation(RotateTransform.AngleProperty, bRotateAnimation, HandoffBehavior.Compose),
				DispatcherPriority.Background);
			((TextBlock)sender).Dispatcher.Invoke(
				() => ((TextBlock)sender).RenderTransform.BeginAnimation(RotateTransform.AngleProperty, cRotateAnimation, HandoffBehavior.Compose),
				DispatcherPriority.Background);
			((TextBlock)sender).Dispatcher.Invoke(
				() => ((TextBlock)sender).RenderTransform.BeginAnimation(RotateTransform.AngleProperty, dRotateAnimation, HandoffBehavior.Compose),
				DispatcherPriority.Background);

			Random random = new Random();
			var symbols = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz";
			string[] team =
			{
				"Osmium",
				"Maxim",
				"Vladislav",
				"Eric",
				"Dmitriy",
				"Rayimbek",
				"Artyom",
				"Dalila",
				"Alexandr"
			};

			string randomMember = team[random.Next(0, team.Length)];

			int part = 15;
			int cycles = part * randomMember.Length;

			int count = 0;
			int index = 0;

			for (int i = 0; i <= cycles; i++)
			{
				StringBuilder text = new StringBuilder(randomMember);

				for (int j = index; j < randomMember.Length; j++)
				{
					text[j] = symbols[random.Next(0, symbols.Length)];
				}
				TextBlockProgramName.Dispatcher.Invoke(() => TextBlockProgramName.Text = text.ToString(), DispatcherPriority.Background);

				count++;
				if (count == part)
				{
					text[index] = randomMember[index];
					count = 0;
					index++;
				}
				Thread.Sleep(25);
			}
		}
	}
}
