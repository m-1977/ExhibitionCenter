using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace ExhibitionCenter.Services
{
    public enum AppLanguage
    {
        Arabic,
        English
    }


    public static class LanguageService
    {
        private static readonly JsonSerializerOptions
            JsonOptions =
                new()
                {
                    WriteIndented = true
                };


        private static LanguageContentData?
            _content;


        public static AppLanguage Current
            { get; private set; } =
            AppLanguage.Arabic;


        private static string RootFolder =>
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "ExhibitionCenter");


        private static string PreferencePath =>
            Path.Combine(
                RootFolder,
                "language.txt");


        private static string ContentPath =>
            Path.Combine(
                RootFolder,
                "language-content.json");


        // =====================================================
        // LANGUAGE PREFERENCE
        // =====================================================

        public static AppLanguage LoadPreference()
        {
            Directory.CreateDirectory(
                RootFolder);


            try
            {
                if (File.Exists(
                        PreferencePath))
                {
                    string value =
                        File.ReadAllText(
                                PreferencePath)
                            .Trim();


                    Current =
                        string.Equals(
                            value,
                            "en",
                            StringComparison.OrdinalIgnoreCase)
                        ? AppLanguage.English
                        : AppLanguage.Arabic;
                }
            }
            catch
            {
                Current =
                    AppLanguage.Arabic;
            }


            return Current;
        }


        public static void SetLanguage(
            AppLanguage language)
        {
            Current =
                language;


            Directory.CreateDirectory(
                RootFolder);


            try
            {
                File.WriteAllText(
                    PreferencePath,
                    language ==
                    AppLanguage.English
                        ? "en"
                        : "ar");
            }
            catch
            {
            }
        }


        // =====================================================
        // CONTENT
        // =====================================================

        public static LanguageContentData
            GetContent()
        {
            if (_content is not null)
            {
                return _content;
            }


            Directory.CreateDirectory(
                RootFolder);


            try
            {
                if (File.Exists(
                        ContentPath))
                {
                    string json =
                        File.ReadAllText(
                            ContentPath);


                    _content =
                        JsonSerializer
                            .Deserialize<LanguageContentData>(
                                json,
                                JsonOptions);
                }
            }
            catch
            {
                _content =
                    null;
            }


            _content ??=
                new LanguageContentData();


            EnsureDefaultCategoryTranslations(
                _content);


            SaveContent(
                _content);


            return _content;
        }


        public static void SaveContent(
            LanguageContentData content)
        {
            _content =
                content;


            Directory.CreateDirectory(
                RootFolder);


            try
            {
                string json =
                    JsonSerializer.Serialize(
                        content,
                        JsonOptions);


                File.WriteAllText(
                    ContentPath,
                    json);
            }
            catch
            {
            }
        }


        // =====================================================
        // CATEGORY
        // =====================================================

        public static string CategoryName(
            string key,
            string arabicFallback)
        {
            if (Current ==
                AppLanguage.Arabic)
            {
                return arabicFallback;
            }


            LanguageContentData content =
                GetContent();


            if (content.Categories.TryGetValue(
                    key,
                    out var text) &&
                !string.IsNullOrWhiteSpace(
                    text.NameEnglish))
            {
                return text.NameEnglish;
            }


            return arabicFallback;
        }


        public static string CategorySubtitle(
            string key,
            string arabicFallback)
        {
            if (Current ==
                AppLanguage.Arabic)
            {
                return arabicFallback;
            }


            LanguageContentData content =
                GetContent();


            if (content.Categories.TryGetValue(
                    key,
                    out var text) &&
                !string.IsNullOrWhiteSpace(
                    text.SubtitleEnglish))
            {
                return text.SubtitleEnglish;
            }


            return arabicFallback;
        }


        // =====================================================
        // DEVICE
        // =====================================================

        public static string DeviceName(
            string id,
            string arabicFallback)
        {
            return GetDeviceText(
                id,
                arabicFallback,
                x => x.NameEnglish);
        }


        public static string DeviceSubtitle(
            string id,
            string arabicFallback)
        {
            return GetDeviceText(
                id,
                arabicFallback,
                x => x.SubtitleEnglish);
        }


        public static string DeviceDescription(
            string id,
            string arabicFallback)
        {
            return GetDeviceText(
                id,
                arabicFallback,
                x => x.DescriptionEnglish);
        }


        public static string DeviceType(
            string id,
            string arabicFallback)
        {
            return GetDeviceText(
                id,
                arabicFallback,
                x => x.TypeEnglish);
        }


        public static string DeviceMaterial(
            string id,
            string arabicFallback)
        {
            return GetDeviceText(
                id,
                arabicFallback,
                x => x.MaterialEnglish);
        }


        private static string GetDeviceText(
            string id,
            string arabicFallback,
            Func<DeviceEnglishText, string> selector)
        {
            if (Current ==
                AppLanguage.Arabic)
            {
                return arabicFallback;
            }


            LanguageContentData content =
                GetContent();


            if (!string.IsNullOrWhiteSpace(
                    id) &&
                content.Devices.TryGetValue(
                    id,
                    out var text))
            {
                string english =
                    selector(text);


                if (!string.IsNullOrWhiteSpace(
                        english))
                {
                    return english;
                }
            }


            // No English translation yet:
            // safely show Arabic instead of blank.
            return arabicFallback;
        }


        // =====================================================
        // DEFAULT ENGLISH CATEGORY TEXT
        // =====================================================

        private static void EnsureDefaultCategoryTranslations(
            LanguageContentData data)
        {
            AddCategoryDefault(
                data,
                "Cameras",
                "Cameras",
                "Moments that shaped history");


            AddCategoryDefault(
                data,
                "Recorders",
                "Recorders",
                "Sounds from a bygone era");


            AddCategoryDefault(
                data,
                "Speakers",
                "Speakers",
                "Sound beyond time");


            AddCategoryDefault(
                data,
                "Phones",
                "Telephones",
                "Communication through generations");


            AddCategoryDefault(
                data,
                "Televisions",
                "Televisions",
                "The picture that entered every home");


            AddCategoryDefault(
                data,
                "Radios",
                "Radio",
                "Voices across the airwaves");
        }


        private static void AddCategoryDefault(
            LanguageContentData data,
            string key,
            string name,
            string subtitle)
        {
            if (!data.Categories.ContainsKey(
                    key))
            {
                data.Categories[key] =
                    new CategoryEnglishText
                    {
                        NameEnglish =
                            name,

                        SubtitleEnglish =
                            subtitle
                    };
            }
        }
    }


    public class LanguageContentData
    {
        public string MainTitleEnglish
            { get; set; } =
            "Technology Through Time";


        public Dictionary<string, CategoryEnglishText>
            Categories
                { get; set; } =
                new();


        public Dictionary<string, DeviceEnglishText>
            Devices
                { get; set; } =
                new();
    }


    public class CategoryEnglishText
    {
        public string NameEnglish
            { get; set; } =
            string.Empty;


        public string SubtitleEnglish
            { get; set; } =
            string.Empty;
    }


    public class DeviceEnglishText
    {
        public string NameEnglish
            { get; set; } =
            string.Empty;


        public string SubtitleEnglish
            { get; set; } =
            string.Empty;


        public string DescriptionEnglish
            { get; set; } =
            string.Empty;


        public string TypeEnglish
            { get; set; } =
            string.Empty;


        public string MaterialEnglish
            { get; set; } =
            string.Empty;
    }
}
