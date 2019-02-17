namespace Translit.Models.Windows
{
    internal interface IMainModel
    {
        int SignIn(string login, string password);
        void LogOut();
        void CompareIds();
    }
}