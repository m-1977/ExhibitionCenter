using System.Collections.Generic;

namespace ExhibitionCenter.Models
{
    public class AboutExhibitionSettings
    {
        public string AccentColor { get; set; } =
            "#E2B258";


        public string ImagePath { get; set; } =
            string.Empty;


        public string TitleAr { get; set; } =
            "معلومات عن المعرض";


        public string TitleEn { get; set; } =
            "About the Exhibition";


        public List<AboutExhibitionNote> Notes
            { get; set; } = new();
    }


    public class AboutExhibitionNote
    {
        public string TitleAr { get; set; } =
            string.Empty;

        public string BodyAr { get; set; } =
            string.Empty;


        public string TitleEn { get; set; } =
            string.Empty;

        public string BodyEn { get; set; } =
            string.Empty;
    }
}
