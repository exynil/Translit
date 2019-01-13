using System.ComponentModel;

namespace Translit.Models.Pages
{
	interface IAboutModel
	{
		string SelectRandomMember();
		event PropertyChangedEventHandler PropertyChanged;
		void Animate();
	}
}
