using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Translit.Models.Pages;
using Translit.Presenters.Pages;

namespace Translit.Views.Pages
{
	public partial class AboutView : IAboutView
	{
		private IAboutPresenter Presenter { get; }

		public AboutView()
		{
			InitializeComponent();
			Presenter = new AboutPresenter(new AboutModel(), this);
		}

		public void ButtonLicense_OnClick(object sender, RoutedEventArgs e)
		{
			Presenter.ShowLicense();

			ColumnDefinitionAbout.Width = new GridLength(0, GridUnitType.Star);
			ColumnDefinitionLicense.Width = new GridLength(1, GridUnitType.Star);
		}

		public void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
		{
			Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
			e.Handled = true;
		}

		public void Page_Loaded(object sender, RoutedEventArgs e)
		{
			TextBlockCopyright.Text = "© " + DateTime.Now.Year + " Osmium";
			TextBlockVersion.Text = GetRes("TextBlockVersion") + ": " + Assembly.GetExecutingAssembly().GetName().Version;
		}

		public void TextBlockProgramName_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			TextBlockProgramName.MouseLeftButtonDown -= TextBlockProgramName_OnMouseLeftButtonDown;

			TextBlockProgramName.HorizontalAlignment = HorizontalAlignment.Left;

			Presenter.OnProgramNameClicked();

			TextBlockProgramName.MouseLeftButtonDown += TextBlockProgramName_OnMouseLeftButtonDown;
		}

		public void SetTextBlockMargin(string text)
		{
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

			var formattedText = new FormattedText(text,
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
		}

		public void UpdateTextInTextBlock(string text)
		{
			TextBlockProgramName.Dispatcher.Invoke(() =>
			{
				TextBlockProgramName.Text = text;
			}, DispatcherPriority.Background);
		}

		public void SetLicense(string text)
		{
			RichTextBoxLicense.AppendText(text);
		}

		public string GetRes(string key)
		{
			return Application.Current.Resources[key].ToString();
		}

		private void ButtonBack_OnClick(object sender, RoutedEventArgs e)
		{
			ColumnDefinitionAbout.Width = new GridLength(1, GridUnitType.Star);
			ColumnDefinitionLicense.Width = new GridLength(0, GridUnitType.Star);
		}
	}
}
