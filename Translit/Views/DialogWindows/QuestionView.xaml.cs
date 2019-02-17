using System.Windows;

namespace Translit.Views.DialogWindows
{
    public partial class QuestionView
    {
        public QuestionView(string question)
        {
            InitializeComponent();
            TextBlockQuestion.Text = question;
        }

        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}