using System.IO;
using System.Text.Json;
using ExhibitionCenter.Models;

namespace ExhibitionCenter.Services
{
    public class ClockScreenSettingsService
    {
        private static readonly JsonSerializerOptions Options =
            new()
            {
                WriteIndented = true
            };


        private static string FilePath =>
            Path.Combine(
                AppPaths.SettingsDirectory,
                "clock-screen-settings.json");


        public ClockScreenSettings Load()
        {
            AppPaths.EnsureDirectories();

            if (!File.Exists(FilePath))
                return new ClockScreenSettings();

            try
            {
                string json =
                    File.ReadAllText(FilePath);

                return
                    JsonSerializer.Deserialize<ClockScreenSettings>(
                        json,
                        Options)
                    ?? new ClockScreenSettings();
            }
            catch
            {
                return new ClockScreenSettings();
            }
        }


        public void Save(
            ClockScreenSettings settings)
        {
            AppPaths.EnsureDirectories();

            string json =
                JsonSerializer.Serialize(
                    settings,
                    Options);

            File.WriteAllText(
                FilePath,
                json);
        }
    }
}
