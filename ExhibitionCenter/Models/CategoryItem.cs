using ExhibitionCenter.Services;

namespace ExhibitionCenter.Models
{
    public class CategoryItem
    {
        public string Key
            { get; set; } =
            string.Empty;


        public string Icon
            { get; set; } =
            string.Empty;


        // Arabic master text
        public string Name
            { get; set; } =
            string.Empty;


        public string Subtitle
            { get; set; } =
            string.Empty;


        // Main cover image shown on Home
        public string ImagePath
            { get; set; } =
            string.Empty;


        // Visitor display text
        public string DisplayName =>
            LanguageService.CategoryName(
                Key,
                Name);


        public string DisplaySubtitle =>
            LanguageService.CategorySubtitle(
                Key,
                Subtitle);
    }
}
