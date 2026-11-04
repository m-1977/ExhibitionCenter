using System.Collections.Generic;

namespace ExhibitionCenter.Models
{
    public class CatalogData
    {
        public List<CategoryItem> Categories
            { get; set; } = new();

        public List<ExhibitItem> Devices
            { get; set; } = new();
    }
}
