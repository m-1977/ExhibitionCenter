using System.IO;
using System.Text.Json;
using ExhibitionCenter.Models;

namespace ExhibitionCenter.Services
{
    public class DeviceSettingsService
    {
        private readonly JsonSerializerOptions _options = new()
        {
            WriteIndented = true
        };


        private string FilePath =>
            Path.Combine(
                AppPaths.SettingsDirectory,
                "device-settings.json");


        public DeviceSettings Load()
        {
            AppPaths.EnsureDirectories();


            if (!File.Exists(FilePath))
            {
                return new DeviceSettings();
            }


            try
            {
                string json =
                    File.ReadAllText(FilePath);


                return JsonSerializer
                           .Deserialize<DeviceSettings>(
                               json,
                               _options)
                       ?? new DeviceSettings();
            }
            catch
            {
                return new DeviceSettings();
            }
        }


        public void Save(
            DeviceSettings settings)
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


        public static string MakeStableId(
            string category,
            string originalName)
        {
            return $"{category}|{originalName}";
        }
    }
}
