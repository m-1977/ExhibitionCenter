using System.Collections.Generic;

namespace ExhibitionCenter.Models
{
    public class DeviceSettings
    {
        public Dictionary<string, DeviceItemSetting> Devices
            { get; set; } = new();
    }


    public class DeviceItemSetting
    {
        public string Name { get; set; } = string.Empty;

        public string Subtitle { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Year { get; set; } = string.Empty;

        public string Company { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public string Material { get; set; } = string.Empty;

        public string ImagePath { get; set; } = string.Empty;

        public string VideoPath { get; set; } = string.Empty;
    }
}
