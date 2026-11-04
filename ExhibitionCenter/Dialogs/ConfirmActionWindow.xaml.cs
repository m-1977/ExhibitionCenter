using System.Windows;

namespace ExhibitionCenter.Dialogs
{
    public partial class ConfirmActionWindow : Window
    {
        public ConfirmActionWindow(
            string title,
            string message)
        {
            InitializeComponent();

            TitleText.Text =
                title;

            MessageText.Text =
                message;
        }


        private void Yes_Click(
            object sender,
            RoutedEventArgs e)
        {
            DialogResult = true;

            Close();
        }


        private void No_Click(
            object sender,
            RoutedEventArgs e)
        {
            DialogResult = false;

            Close();
        }
    }
}
