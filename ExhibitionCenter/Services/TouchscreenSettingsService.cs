using System;
using System.IO;
using System.Text.Json;
using ExhibitionCenter.Models;

namespace ExhibitionCenter.Services
{
    public sealed class TouchscreenSettingsService
    {
        private static readonly string SettingsFolder =
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "ExhibitionCenter");

        private static readonly string SettingsPath =
            Path.Combine(
                SettingsFolder,
                "touchscreen-settings.json");


        public TouchscreenSettings Load()
        {
            try
            {
                if (!File.Exists(SettingsPath))
                    return new TouchscreenSettings();

                string json =
                    File.ReadAllText(SettingsPath);

                return
                    JsonSerializer.Deserialize<TouchscreenSettings>(json)
                    ?? new TouchscreenSettings();
            }
            catch
            {
                return new TouchscreenSettings();
            }
        }


        public void Save(
            TouchscreenSettings settings)
        {
            Directory.CreateDirectory(
                SettingsFolder);

            string json =
                JsonSerializer.Serialize(
                    settings,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });

            File.WriteAllText(
                SettingsPath,
                json);
        }
    }
}
