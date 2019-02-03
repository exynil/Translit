using Translit.Entity;

namespace Translit.Models.Windows
{
	interface IMainModel
	{
		bool SignIn(string login, string password);
		User GetUser();
		void DeleteToken();
		void DeleteUserFromSettings();
		void MacAddressComparison();
	}
}
