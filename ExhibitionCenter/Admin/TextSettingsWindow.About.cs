using System.Windows;

namespace ExhibitionCenter.Admin
{
    public partial class TextSettingsWindow
    {
        private void OpenAboutExhibitionSettings_Click(
            object sender,
            RoutedEventArgs e)
        {
            var window =
                new AboutExhibitionSettingsWindow
                {
                    Owner = this
                };


            window.ShowDialog();
        }
    }
}
