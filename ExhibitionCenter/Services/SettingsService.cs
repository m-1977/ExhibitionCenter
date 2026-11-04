using System.IO;
using System.Text.Json;
using ExhibitionCenter.Models;

namespace ExhibitionCenter.Services
{
    public class SettingsService
    {
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true
        };

        public AppSettings Load()
        {
            AppPaths.EnsureDirectories();

            if (!File.Exists(AppPaths.SettingsFile))
                return new AppSettings();

            try
            {
                string json = File.ReadAllText(AppPaths.SettingsFile);

                return JsonSerializer.Deserialize<AppSettings>(json)
                       ?? new AppSettings();
            }
            catch
            {
                return new AppSettings();
            }
        }

        public void Save(AppSettings settings)
        {
            AppPaths.EnsureDirectories();

            string json = JsonSerializer.Serialize(settings, _jsonOptions);

            File.WriteAllText(AppPaths.SettingsFile, json);
        }
    }
}
