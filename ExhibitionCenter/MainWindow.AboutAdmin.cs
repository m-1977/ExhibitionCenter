using System.Windows;
using ExhibitionCenter.Admin;

namespace ExhibitionCenter
{
    public partial class MainWindow
    {
        private void OpenAboutExhibitionAdmin_Click(
            object sender,
            RoutedEventArgs e)
        {
            var editor = new AboutExhibitionSettingsWindow
            {
                Owner = this
            };

            editor.ShowDialog();
        }
    }
}
