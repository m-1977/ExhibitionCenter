using ExhibitionCenter.Services;

namespace ExhibitionCenter.Models
{
    public class ExhibitItem
    {
        public string Id
            { get; set; } =
            string.Empty;


        public string Category
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


        public string Description
            { get; set; } =
            string.Empty;


        public string Year
            { get; set; } =
            string.Empty;


        public string Company
            { get; set; } =
            string.Empty;


        public string Type
            { get; set; } =
            string.Empty;


        public string Material
            { get; set; } =
            string.Empty;


        public string ImagePath
            { get; set; } =
            string.Empty;


        public string VideoPath
            { get; set; } =
            string.Empty;


        // Visitor display text
        public string DisplayName =>
            LanguageService.DeviceName(
                Id,
                Name);


        public string DisplaySubtitle =>
            LanguageService.DeviceSubtitle(
                Id,
                Subtitle);


        public string DisplayDescription =>
            LanguageService.DeviceDescription(
                Id,
                Description);


        public string DisplayType =>
            LanguageService.DeviceType(
                Id,
                Type);


        public string DisplayMaterial =>
            LanguageService.DeviceMaterial(
                Id,
                Material);
    }
}
