using Translit.Entity;

namespace Translit.Models.Windows
{
	interface IMainWindowModel
	{
		string Login { get; set; }
		string Password { get; set; }
		bool IsLoginOrPasswordEmpty();
		User SignIn();
		User GetUser();
		void DeleteToken();
		void DeleteUserFromSettings();
		void MacAddressComparison();
	}
}
