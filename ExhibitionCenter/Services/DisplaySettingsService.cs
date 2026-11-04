using System.IO;
using System.Text.Json;
using ExhibitionCenter.Models;

namespace ExhibitionCenter.Services
{
    public class DisplaySettingsService
    {
        private readonly JsonSerializerOptions _options = new()
        {
            WriteIndented = true
        };

        private string FilePath =>
            Path.Combine(
                AppPaths.SettingsDirectory,
                "display-settings.json");

        public DisplaySettings Load()
        {
            AppPaths.EnsureDirectories();

            if (!File.Exists(FilePath))
                return new DisplaySettings();

            try
            {
                string json =
                    File.ReadAllText(FilePath);

                return JsonSerializer
                           .Deserialize<DisplaySettings>(
                               json,
                               _options)
                       ?? new DisplaySettings();
            }
            catch
            {
                return new DisplaySettings();
            }
        }

        public void Save(DisplaySettings settings)
        {
            AppPaths.EnsureDirectories();

            string json =
                JsonSerializer.Serialize(
                    settings,
                    _options);

            File.WriteAllText(
                FilePath,
                json);
        }
    }
}
