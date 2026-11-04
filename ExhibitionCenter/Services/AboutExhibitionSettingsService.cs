using System;
using System.IO;
using System.Text.Json;
using ExhibitionCenter.Models;

namespace ExhibitionCenter.Services
{
    public class AboutExhibitionSettingsService
    {
        private static readonly JsonSerializerOptions Options =
            new()
            {
                WriteIndented = true
            };


        private static string FilePath =>
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "ExhibitionCenter",
                "about-exhibition.json");


        public AboutExhibitionSettings Load()
        {
            AboutExhibitionSettings settings;


            try
            {
                if (File.Exists(FilePath))
                {
                    string json =
                        File.ReadAllText(FilePath);


                    settings =
                        JsonSerializer.Deserialize
                            <AboutExhibitionSettings>(
                                json,
                                Options)
                        ?? CreateDefault();
                }
                else
                {
                    settings =
                        CreateDefault();
                }
            }
            catch
            {
                settings =
                    CreateDefault();
            }


            EnsureFourNotes(settings);

            return settings;
        }


        public void Save(
            AboutExhibitionSettings settings)
        {
            EnsureFourNotes(settings);


            string? directory =
                Path.GetDirectoryName(FilePath);


            if (!string.IsNullOrWhiteSpace(
                    directory))
            {
                Directory.CreateDirectory(
                    directory);
            }


            string json =
                JsonSerializer.Serialize(
                    settings,
                    Options);


            File.WriteAllText(
                FilePath,
                json);
        }


        private static void EnsureFourNotes(
            AboutExhibitionSettings settings)
        {
            settings.Notes ??=
                new();


            while (settings.Notes.Count < 4)
            {
                settings.Notes.Add(
                    new AboutExhibitionNote());
            }


            if (settings.Notes.Count > 4)
            {
                settings.Notes =
                    settings.Notes
                        .GetRange(0, 4);
            }
        }


        private static AboutExhibitionSettings
            CreateDefault()
        {
            var settings =
                new AboutExhibitionSettings();


            settings.Notes.Add(
                new AboutExhibitionNote
                {
                    TitleAr =
                        "مرحباً بكم في معرضنا",

                    BodyAr =
                        "يأخذكم هذا المعرض في رحلة تفاعلية عبر تاريخ التقنية، لاستكشاف الأجهزة والابتكارات التي ساهمت في تشكيل حياتنا عبر الزمن.",

                    TitleEn =
                        "Welcome to Our Exhibition",

                    BodyEn =
                        "Explore an interactive journey through the history of technology and discover the devices and innovations that shaped the way we live."
                });


            settings.Notes.Add(
                new AboutExhibitionNote());

            settings.Notes.Add(
                new AboutExhibitionNote());

            settings.Notes.Add(
                new AboutExhibitionNote());


            return settings;
        }
    }
}
