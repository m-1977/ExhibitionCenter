using System.IO;
using System.Text.Json;
using ExhibitionCenter.Models;

namespace ExhibitionCenter.Services
{
    public sealed class PowerScreenSettingsService
    {
        private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
        private static string FilePath => Path.Combine(AppPaths.SettingsDirectory, "power-screen-settings.json");

        public PowerScreenSettings Load()
        {
            try
            {
                if (!File.Exists(FilePath))
                    return new PowerScreenSettings();

                return JsonSerializer.Deserialize<PowerScreenSettings>(File.ReadAllText(FilePath), Options)
                    ?? new PowerScreenSettings();
            }
            catch
            {
                return new PowerScreenSettings();
            }
        }

        public void Save(PowerScreenSettings settings)
        {
            AppPaths.EnsureDirectories();
            File.WriteAllText(FilePath, JsonSerializer.Serialize(settings, Options));
        }
    }
}
