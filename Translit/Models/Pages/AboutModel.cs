using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace Translit.Models.Pages
{
	public class AboutModel : IAboutModel, INotifyPropertyChanged
	{
		private string RandomMember { get; set; }
		private string _modifiedMember;

		public string ModifiedMember
		{
			get => _modifiedMember;
			set
			{
				_modifiedMember = value;
				OnPropertyChanged();
			}
		}
		public event PropertyChangedEventHandler PropertyChanged;
		public void OnPropertyChanged([CallerMemberName]string prop = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
		}

		public string SelectRandomMember()
		{
			var random = new Random();
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

			RandomMember = team[random.Next(0, team.Length)];
			return RandomMember;
		}

		public void Animate()
		{
			var random = new Random();
			const string symbols = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
			const int part = 10;
			var cycles = part * RandomMember.Length;

			var count = 0;
			var index = 0;

			for (var i = 0; i <= cycles; i++)
			{
				var member = new StringBuilder(RandomMember);

				for (var j = index; j < RandomMember.Length; j++)
				{
					if (j == 0)
					{
						member[j] = symbols[random.Next(0, symbols.Length / 2)];
						continue;
					}
					member[j] = symbols[random.Next(symbols.Length / 2, symbols.Length)];
				}

				count++;

				if (count == part)
				{
					member[index] = RandomMember[index];
					count = 0;
					index++;
				}

				ModifiedMember = member.ToString();

				Thread.Sleep(30);
			}
		}
	}
}
