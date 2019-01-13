using Translit.Entity;

namespace Translit.Models.Windows
{
	interface IMainWindowModel
	{
		bool SignIn(string login, string password);
		User GetUser();
		void DeleteToken();
		void DeleteUserFromSettings();
		void MacAddressComparison();
	}
}
