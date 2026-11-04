using System.Windows;
using ExhibitionCenter.Models;
using ExhibitionCenter.Services;

namespace ExhibitionCenter.Admin
{
    public partial class TouchscreenSettingsWindow : Window
    {
        private readonly TouchscreenSettingsService
            _service;


        public TouchscreenSettingsWindow(
            TouchscreenSettingsService service)
        {
            InitializeComponent();

            _service = service;

            TouchscreenSettings settings =
                _service.Load();

            TouchscreenToggle.IsChecked =
                settings.Enabled;
        }


        private void Save_Click(
            object sender,
            RoutedEventArgs e)
        {
            _service.Save(
                new TouchscreenSettings
                {
                    Enabled =
                        TouchscreenToggle.IsChecked == true
                });

            DialogResult = true;
        }


        private void Cancel_Click(
            object sender,
            RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
