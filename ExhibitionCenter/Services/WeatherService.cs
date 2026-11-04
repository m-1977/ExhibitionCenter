using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ExhibitionCenter.Services
{
    public sealed class WeatherReading
    {
        public string City { get; set; } = string.Empty;
        public double TemperatureCelsius { get; set; }
        public int WeatherCode { get; set; }
        public DateTimeOffset FetchedAtUtc { get; set; }
    }

    public sealed class WeatherService
    {
        private static readonly HttpClient Client = new()
        {
            Timeout = TimeSpan.FromSeconds(8)
        };

        private static readonly TimeSpan RefreshInterval = TimeSpan.FromMinutes(15);

        private static string CachePath => Path.Combine(
            AppPaths.SettingsDirectory, "clock-weather-cache.json");

        public WeatherReading? GetCached(string city)
        {
            try
            {
                if (!File.Exists(CachePath))
                    return null;

                WeatherReading? reading = JsonSerializer.Deserialize<WeatherReading>(
                    File.ReadAllText(CachePath));

                return reading is not null &&
                       string.Equals(reading.City, city.Trim(),
                           StringComparison.OrdinalIgnoreCase) &&
                       double.IsFinite(reading.TemperatureCelsius) &&
                       reading.FetchedAtUtc != default
                    ? reading
                    : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<WeatherReading?> GetCurrentAsync(
            string city, CancellationToken cancellationToken = default)
        {
            city = city.Trim();
            WeatherReading? cached = GetCached(city);
            if (cached is not null &&
                DateTimeOffset.UtcNow - cached.FetchedAtUtc < RefreshInterval)
            {
                return cached;
            }

            try
            {
                string searchName = city is "الدوحة" or "Doha" ? "Doha" : city;
                string countryFilter = searchName == "Doha" ? "&countryCode=QA" : "";
                string geocodingUrl =
                    "https://geocoding-api.open-meteo.com/v1/search?name=" +
                    Uri.EscapeDataString(searchName) +
                    "&count=1" + countryFilter;

                using JsonDocument places = await GetJsonAsync(
                    geocodingUrl, cancellationToken);
                JsonElement results = places.RootElement.GetProperty("results");
                if (results.GetArrayLength() == 0)
                    return cached;

                JsonElement place = results[0];
                string latitude = place.GetProperty("latitude").GetDouble()
                    .ToString(CultureInfo.InvariantCulture);
                string longitude = place.GetProperty("longitude").GetDouble()
                    .ToString(CultureInfo.InvariantCulture);

                string forecastUrl =
                    "https://api.open-meteo.com/v1/forecast?latitude=" +
                    latitude + "&longitude=" + longitude +
                    "&current=temperature_2m,weather_code&temperature_unit=celsius";

                using JsonDocument forecast = await GetJsonAsync(
                    forecastUrl, cancellationToken);
                JsonElement current = forecast.RootElement.GetProperty("current");
                double temperature = current.GetProperty("temperature_2m").GetDouble();
                if (!double.IsFinite(temperature))
                    return cached;

                var reading = new WeatherReading
                {
                    City = city,
                    TemperatureCelsius = temperature,
                    WeatherCode = current.GetProperty("weather_code").GetInt32(),
                    FetchedAtUtc = DateTimeOffset.UtcNow
                };

                try
                {
                    AppPaths.EnsureDirectories();
                    await File.WriteAllTextAsync(CachePath,
                        JsonSerializer.Serialize(reading), cancellationToken);
                }
                catch (Exception)
                {
                    // A cache write failure must not hide a valid live reading.
                }

                return reading;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return cached;
            }
            catch (Exception)
            {
                return cached;
            }
        }

        private static async Task<JsonDocument> GetJsonAsync(
            string url, CancellationToken cancellationToken)
        {
            using HttpResponseMessage response = await Client.GetAsync(
                url, cancellationToken);
            response.EnsureSuccessStatusCode();
            await using Stream stream = await response.Content.ReadAsStreamAsync(
                cancellationToken);
            return await JsonDocument.ParseAsync(stream,
                cancellationToken: cancellationToken);
        }
    }
}
