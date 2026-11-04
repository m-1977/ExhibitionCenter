using System;
using System.IO;

namespace ExhibitionCenter.Services
{
    public static class AppPaths
    {
        public static string SettingsDirectory =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ExhibitionCenter");

        public static string SettingsFile =>
            Path.Combine(SettingsDirectory, "settings.json");

        public static void EnsureDirectories()
        {
            Directory.CreateDirectory(SettingsDirectory);
        }
    }
}
