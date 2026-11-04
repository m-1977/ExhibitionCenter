using System.Windows;
using ExhibitionCenter.Models;
using ExhibitionCenter.Services;

namespace ExhibitionCenter.Admin
{
    public partial class ClockSettingsWindow : Window
    {
        private readonly ClockScreenSettingsService _service =
            new();

        private ClockScreenSettings _settings;


        public ClockSettingsWindow()
        {
            InitializeComponent();

            _settings =
                _service.Load();

            LoadSettings();
        }


        private void LoadSettings()
        {
            EnableClockToggle.IsChecked =
                _settings.Enabled;

            ShowWeatherToggle.IsChecked =
                _settings.ShowWeather;

            CityTextBox.Text =
                string.IsNullOrWhiteSpace(
                    _settings.City)
                ? "الدوحة"
                : _settings.City;


            Idle1Option.IsChecked =
                _settings.IdleMinutes == 1;

            Idle3Option.IsChecked =
                _settings.IdleMinutes == 3;

            Idle5Option.IsChecked =
                _settings.IdleMinutes == 5 ||
                (_settings.IdleMinutes != 1 &&
                 _settings.IdleMinutes != 3 &&
                 _settings.IdleMinutes != 10);

            Idle10Option.IsChecked =
                _settings.IdleMinutes == 10;
        }


        private void Save_Click(
            object sender,
            RoutedEventArgs e)
        {
            _settings.Enabled =
                EnableClockToggle.IsChecked == true;

            _settings.ShowWeather =
                ShowWeatherToggle.IsChecked == true;

            _settings.City =
                string.IsNullOrWhiteSpace(
                    CityTextBox.Text)
                ? "الدوحة"
                : CityTextBox.Text.Trim();


            _settings.IdleMinutes =
                Idle1Option.IsChecked == true
                    ? 1
                    : Idle3Option.IsChecked == true
                        ? 3
                        : Idle10Option.IsChecked == true
                            ? 10
                            : 5;


            _service.Save(
                _settings);


            DialogResult =
                true;

            Close();
        }


        private void Cancel_Click(
            object sender,
            RoutedEventArgs e)
        {
            DialogResult =
                false;

            Close();
        }
    }
}

