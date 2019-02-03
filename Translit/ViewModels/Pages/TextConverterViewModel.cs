using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Translit.Entity;
using Translit.Models.Pages;
using Translit.Properties;

namespace Translit.ViewModels.Pages
{
	class TextConverterViewModel : INotifyPropertyChanged
	{
		private string _cyryllic;
		private double _fontSize;
		public event PropertyChangedEventHandler PropertyChanged;
		public TextConverterModel Model { get; set; }

		public string Cyryllic
		{
			get => _cyryllic;
			set
			{
				_cyryllic = value;

				Transliterate();
			}
		}

		public string Latin { get; set; }

		public double FontSize
		{
			get => _fontSize;
			set
			{
				_fontSize = value;
				Settings.Default.TextConverterFontSize = value;
			}
		}

		public TextConverterViewModel()
		{
			Model = new TextConverterModel();
			FontSize = Settings.Default.TextConverterFontSize;
		}

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private void Transliterate()
		{
			Task.Factory.StartNew(() =>
			{
				Latin = Model.Transliterate(Cyryllic) ?? GetRes("SnackBarDatabaseNotFound");
			});
		}

		public ICommand Clear
		{
			get
			{
				return new DelegateCommand(o => { Cyryllic = Latin = ""; });
			}
		}

		public ICommand Copy
		{
			get
			{
				return new DelegateCommand(o =>
				{

					Clipboard.SetData(DataFormats.UnicodeText, Latin);
				});
			}
		}

		// Получение ресурса по ключу
		public string GetRes(string key)
		{
			return Application.Current.Resources[key].ToString();
		}
	}
}
