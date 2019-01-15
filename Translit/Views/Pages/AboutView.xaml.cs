﻿using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Translit.Models.Pages;
using Translit.Presenters.Pages;
using Application = System.Windows.Application;

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

		public void Page_Loaded(object sender, RoutedEventArgs e)
		{
			var version = Assembly.GetExecutingAssembly().GetName().Version;

			TextBlockCopyright.Text = "© Osmium 2018-" + DateTime.Now.Year;

			TextBlockVersion.SetResourceReference(TextBlock.TextProperty, "TextBlockVersion");

			TextBlockVersionResult.Text = " " + version.Major + "." + version.Minor + " (" + version.Build + ")";

			Loaded -= Page_Loaded;
		}

		public void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
		{
			Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
			e.Handled = true;
		}

		public async void TextBlockProgramName_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			TextBlockProgramName.MouseLeftButtonDown -= TextBlockProgramName_OnMouseLeftButtonDown;

			TextBlockProgramName.HorizontalAlignment = HorizontalAlignment.Left;

			await Task.Factory.StartNew(() =>
			{
				Presenter.OnProgramNameClicked();
			});

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

		public string GetRes(string key)
		{
			return Application.Current.Resources[key].ToString();
		}
	}
}
