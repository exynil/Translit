﻿using System.ComponentModel;
using Translit.Models.Pages;
using Translit.Views.Pages;

namespace Translit.Presenters.Pages
{
	public class AboutPresenter
	{
		public AboutModel Model { get; }
		public AboutView View { get; }

		public AboutPresenter(AboutModel model, AboutView view)
		{
			Model = model;
			View = view;
		}

		public void OnProgramNameClicked()
		{
			View.SetTextBlockMargin(Model.SelectRandomMember());
			Model.PropertyChanged += TrackProperties;
			Model.Animate();
		}

		public void TrackProperties(object sender, PropertyChangedEventArgs e)
		{
			View.UpdateTextInTextBlock(((AboutModel)sender).ModifiedMember);
		}

		public void ShowLicense()
		{
			View.SetLicense(Model.ReadLicense());
		}
	}
}