using System.Windows;
using ExhibitionCenter.Admin;

namespace ExhibitionCenter
{
    public partial class MainWindow
    {
        private void OpenStartupSettings_Click(object sender, RoutedEventArgs e)
        {
            var window = new StartupSettingsWindow
            {
                Owner = this
            };

            window.ShowDialog();
        }
    }
}
