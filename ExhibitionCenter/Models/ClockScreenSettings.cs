namespace ExhibitionCenter.Models
{
    public class ClockScreenSettings
    {
        public bool Enabled { get; set; } = true;

        public int IdleMinutes { get; set; } = 5;

        public bool ShowWeather { get; set; } = true;

        public string City { get; set; } = "الدوحة";

        public string TemperatureUnit { get; set; } = "C";
    }
}
