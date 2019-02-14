namespace Translit.Models.Windows
{
    interface IMainModel
	{
		int SignIn(string login, string password);
	    void LogOut();
		void CompareIds();
	}
}
