using System.Windows;
using ExhibitionCenter.Admin;

namespace ExhibitionCenter
{
    public partial class MainWindow
    {
        private void OpenClockSettings_Click(
            object sender,
            RoutedEventArgs e)
        {
            var window =
                new ClockSettingsWindow
                {
                    Owner = this
                };

            bool? result =
                window.ShowDialog();

            if (result == true)
            {
                ReloadClockScreenSettings();
            }
        }
    }
}

