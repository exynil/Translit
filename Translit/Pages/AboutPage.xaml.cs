using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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

		private async void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			TextBlockProgramName.MouseLeftButtonDown -= UIElement_OnMouseLeftButtonDown;
			await Task.Factory.StartNew(() =>
			{
				TextBlockProgramName.Dispatcher.Invoke(() =>
					{
						TextBlockProgramName.HorizontalAlignment = HorizontalAlignment.Left;
					}, DispatcherPriority.Background);
				var random = new Random();
				const string symbols = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
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
				var randomMember = team[random.Next(0, team.Length)];

				var fontFamily =
					TextBlockProgramName.Dispatcher.Invoke(
						() => (FontFamily)TextBlockProgramName.GetValue(TextBlock.FontFamilyProperty),
						DispatcherPriority.Background);

				var fontStyle =
					TextBlockProgramName.Dispatcher.Invoke(
						() => (FontStyle)TextBlockProgramName.GetValue(TextBlock.FontStyleProperty),
						DispatcherPriority.Background);

				var fontWeight =
					TextBlockProgramName.Dispatcher.Invoke(
						() => (FontWeight)TextBlockProgramName.GetValue(TextBlock.FontWeightProperty),
						DispatcherPriority.Background);

				var fontStretch =
					TextBlockProgramName.Dispatcher.Invoke(
						() => (FontStretch)TextBlockProgramName.GetValue(TextBlock.FontStretchProperty),
						DispatcherPriority.Background);

				var fontSize =
					TextBlockProgramName.Dispatcher.Invoke(() => (double)TextBlockProgramName.GetValue(TextBlock.FontSizeProperty),
						DispatcherPriority.Background);

				var formattedText = new FormattedText(randomMember,
					CultureInfo.CurrentCulture,
					FlowDirection.LeftToRight,
					new Typeface(fontFamily,
						fontStyle,
						fontWeight,
						fontStretch),
					fontSize,
					null,
					new NumberSubstitution());

				var size = new Size(formattedText.Width,
					formattedText.Height);

				TextBlockProgramName.Dispatcher.Invoke(() =>
				{
					TextBlockProgramName.Margin = new Thickness((400 - size.Width) / 2, 0, 0, 0);
				}, DispatcherPriority.Background);

				const int part = 10;
				var cycles = part * randomMember.Length;

				var count = 0;
				var index = 0;

				for (var i = 0; i <= cycles; i++)
				{
					var text = new StringBuilder(randomMember);

					for (var j = index; j < randomMember.Length; j++)
					{
						if (j == 0)
						{
							text[j] = symbols[random.Next(0, symbols.Length / 2)];
							continue;
						}
						text[j] = symbols[random.Next(symbols.Length / 2, symbols.Length)];
					}

					TextBlockProgramName.Dispatcher.Invoke(() =>
					{
						TextBlockProgramName.Text = text.ToString();
					}, DispatcherPriority.Background);

					count++;
					if (count == part)
					{
						text[index] = randomMember[index];
						count = 0;
						index++;
					}
					Thread.Sleep(30);
				}
			});
			TextBlockProgramName.MouseLeftButtonDown += UIElement_OnMouseLeftButtonDown;
		}
	}
}
