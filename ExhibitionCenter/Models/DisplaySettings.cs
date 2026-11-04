using System.Collections.Generic;

namespace ExhibitionCenter.Models
{
    public class DisplaySettings
    {
        public string MainTitle { get; set; } =
            "رحلة عبر تاريخ التقنية";

        public Dictionary<string, CategoryTextSetting> Categories
            { get; set; } = new();
    }

    public class CategoryTextSetting
    {
        public string Name { get; set; } = string.Empty;

        public string Subtitle { get; set; } = string.Empty;

        public string ImagePath { get; set; } = string.Empty;
    }
}
