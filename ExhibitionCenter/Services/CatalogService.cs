using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using ExhibitionCenter.Models;

namespace ExhibitionCenter.Services
{
    public static class CatalogService
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
                "catalog.json");


        public static bool Exists =>
            File.Exists(FilePath);


        public static void Save(
            IEnumerable<CategoryItem> categories,
            IDictionary<string, List<ExhibitItem>> catalog)
        {
            string? directory =
                Path.GetDirectoryName(FilePath);


            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }


            var data =
                new CatalogData
                {
                    Categories =
                        categories
                            .Select(CloneCategory)
                            .ToList(),

                    Devices =
                        catalog
                            .SelectMany(x => x.Value)
                            .Select(CloneDevice)
                            .ToList()
                };


            foreach (var device in data.Devices)
            {
                if (string.IsNullOrWhiteSpace(device.Id))
                {
                    device.Id =
                        Guid.NewGuid()
                            .ToString("N");
                }
            }


            string json =
                JsonSerializer.Serialize(
                    data,
                    Options);


            File.WriteAllText(
                FilePath,
                json);
        }


        public static bool LoadInto(
            List<CategoryItem> categories,
            Dictionary<string, List<ExhibitItem>> catalog)
        {
            if (!File.Exists(FilePath))
            {
                return false;
            }


            try
            {
                string json =
                    File.ReadAllText(FilePath);


                CatalogData? data =
                    JsonSerializer.Deserialize<CatalogData>(
                        json,
                        Options);


                if (data is null)
                {
                    return false;
                }


                categories.Clear();


                foreach (var category
                         in data.Categories)
                {
                    categories.Add(
                        CloneCategory(category));
                }


                catalog.Clear();


                foreach (var category
                         in categories)
                {
                    catalog[category.Key] =
                        new List<ExhibitItem>();
                }


                foreach (var device
                         in data.Devices)
                {
                    if (!catalog.TryGetValue(
                            device.Category,
                            out var list))
                    {
                        continue;
                    }


                    list.Add(
                        CloneDevice(device));
                }


                return true;
            }
            catch
            {
                return false;
            }
        }


        private static CategoryItem CloneCategory(
            CategoryItem source)
        {
            return new CategoryItem
            {
                Key =
                    source.Key,

                Icon =
                    source.Icon,

                Name =
                    source.Name,

                Subtitle =
                    source.Subtitle,

                ImagePath =
                    source.ImagePath
            };
        }


        private static ExhibitItem CloneDevice(
            ExhibitItem source)
        {
            return new ExhibitItem
            {
                Id =
                    source.Id,

                Category =
                    source.Category,

                Icon =
                    source.Icon,

                Name =
                    source.Name,

                Subtitle =
                    source.Subtitle,

                Description =
                    source.Description,

                Year =
                    source.Year,

                Company =
                    source.Company,

                Type =
                    source.Type,

                Material =
                    source.Material,

                ImagePath =
                    source.ImagePath,

                VideoPath =
                    source.VideoPath
            };
        }
    }
}
