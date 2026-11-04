using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using ExhibitionCenter.Models;
using ExhibitionCenter.Services;

namespace ExhibitionCenter
{
    public partial class ClockScreenWindow : Window
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct CursorPoint
        {
            public int X;
            public int Y;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out CursorPoint point);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr window);

        private readonly DispatcherTimer _clockTimer = new()
        {
            Interval = TimeSpan.FromSeconds(1)
        };

        private readonly CursorPoint _initialCursorPosition;
        private readonly WeatherService _weatherService = new();
        private readonly CancellationTokenSource _weatherCancellation = new();
        private readonly string _weatherCity;
        private readonly bool _showWeather;
        private readonly bool _useFahrenheit;

        public ClockScreenWindow(ClockScreenSettings settings)
        {
            InitializeComponent();

            _weatherCity = string.IsNullOrWhiteSpace(settings.City)
                ? "الدوحة"
                : settings.City.Trim();
            _showWeather = settings.ShowWeather;
            _useFahrenheit = string.Equals(settings.TemperatureUnit,
                "F", StringComparison.OrdinalIgnoreCase);
            ClockWeatherPanel.Visibility = _showWeather
                ? Visibility.Visible
                : Visibility.Collapsed;
            ClockWeatherCityText.Text = _weatherCity;

            if (_showWeather)
            {
                WeatherReading? cached = _weatherService.GetCached(_weatherCity);
                if (cached is not null)
                    ApplyWeather(cached);

                Loaded += async (_, _) => await RefreshWeatherAsync();
            }

            GetCursorPos(out _initialCursorPosition);
            UpdateClockText();
            _clockTimer.Tick += (_, _) => UpdateClockText();
            _clockTimer.Start();
            Closed += (_, _) =>
            {
                _clockTimer.Stop();
                _weatherCancellation.Cancel();
            };
        }

        private async Task RefreshWeatherAsync()
        {
            WeatherReading? reading = await _weatherService.GetCurrentAsync(
                _weatherCity, _weatherCancellation.Token);
            if (IsVisible && reading is not null)
                ApplyWeather(reading);
        }

        private void ApplyWeather(WeatherReading reading)
        {
            double temperature = _useFahrenheit
                ? reading.TemperatureCelsius * 9 / 5 + 32
                : reading.TemperatureCelsius;
            string unit = _useFahrenheit ? "F" : "C";

            ClockTemperatureText.Text =
                Math.Round(temperature, MidpointRounding.AwayFromZero)
                    .ToString("0", CultureInfo.InvariantCulture) + "°" + unit;
            ClockTemperatureText.Visibility = Visibility.Visible;
            ClockWeatherIcon.Text = reading.WeatherCode switch
            {
                0 or 1 => "☀",
                2 or 3 or 45 or 48 => "☁",
                >= 95 => "⚡",
                >= 51 and <= 82 => "☂",
                _ => "☁"
            };
        }

        public void BringToForeground()
        {
            IntPtr handle = new WindowInteropHelper(this).Handle;
            if (handle != IntPtr.Zero)
                SetForegroundWindow(handle);

            Activate();
            Focus();
        }

        private void UpdateClockText()
        {
            DateTime now = DateTime.Now;
            ClockTimeText.Text = now.ToString("hh:mm", CultureInfo.InvariantCulture);
            ClockAmPmText.Text = now.ToString("tt", CultureInfo.InvariantCulture);
            ClockDateText.Text = now.ToString(
                "dddd، d MMMM yyyy", CultureInfo.GetCultureInfo("ar-QA"));
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            base.OnPreviewMouseMove(e);

            // Showing a window can raise MouseMove without physical movement.
            if (GetCursorPos(out CursorPoint position) &&
                (position.X != _initialCursorPosition.X ||
                 position.Y != _initialCursorPosition.Y))
            {
                Close();
            }
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);
            Close();
        }

        protected override void OnPreviewTouchDown(TouchEventArgs e)
        {
            base.OnPreviewTouchDown(e);
            Close();
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            Close();
        }
    }
}
