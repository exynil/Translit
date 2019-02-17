using System.Windows;

namespace Translit.Views.DialogWindows
{
    public partial class EditDialogView
    {
        public EditDialogView(string cyryllic, string latin)
        {
            InitializeComponent();
            TextBoxEditCyryllic.Text = cyryllic;
            TextBoxEditLatin.Text = latin;
        }

        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            if (TextBoxEditCyryllic.Text != string.Empty && TextBoxEditLatin.Text != string.Empty)
                DialogResult = true;
            else
                TextBlockMessage.Text = GetRes("TextBlockPleaseFillInAllFields");
        }

        // Получение ресурса по ключу
        public string GetRes(string key)
        {
            return Application.Current.Resources[key].ToString();
        }
    }
}