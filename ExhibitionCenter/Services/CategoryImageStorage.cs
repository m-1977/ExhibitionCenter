using System;
using System.IO;
using System.Linq;

namespace ExhibitionCenter.Services
{
    public static class CategoryImageStorage
    {
        public static string DirectoryPath
        {
            get
            {
                string path =
                    Path.Combine(
                        Environment.GetFolderPath(
                            Environment.SpecialFolder.LocalApplicationData),
                        "ExhibitionCenter",
                        "CategoryImages");

                Directory.CreateDirectory(path);

                return path;
            }
        }


        public static string Store(
            string categoryKey,
            string sourcePath)
        {
            if (string.IsNullOrWhiteSpace(sourcePath))
                return string.Empty;

            if (!File.Exists(sourcePath))
                return string.Empty;


            string sourceFullPath =
                Path.GetFullPath(sourcePath);

            string storageFullPath =
                Path.GetFullPath(DirectoryPath);


            // Already stored by Exhibition Center
            if (sourceFullPath.StartsWith(
                    storageFullPath,
                    StringComparison.OrdinalIgnoreCase))
            {
                return sourceFullPath;
            }


            string safeKey =
                MakeSafeFileName(categoryKey);


            string extension =
                Path.GetExtension(sourcePath)
                    .ToLowerInvariant();


            string fileName =
                $"{safeKey}_{DateTime.Now:yyyyMMddHHmmssfff}{extension}";


            string destination =
                Path.Combine(
                    DirectoryPath,
                    fileName);


            File.Copy(
                sourcePath,
                destination,
                true);


            // Remove older cover images for this category.
            foreach (string oldFile in
                     Directory.GetFiles(
                         DirectoryPath,
                         $"{safeKey}_*.*"))
            {
                if (!string.Equals(
                        Path.GetFullPath(oldFile),
                        Path.GetFullPath(destination),
                        StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        File.Delete(oldFile);
                    }
                    catch
                    {
                    }
                }
            }


            return destination;
        }


        public static void Remove(
            string categoryKey)
        {
            string safeKey =
                MakeSafeFileName(categoryKey);


            foreach (string file in
                     Directory.GetFiles(
                         DirectoryPath,
                         $"{safeKey}_*.*"))
            {
                try
                {
                    File.Delete(file);
                }
                catch
                {
                }
            }
        }


        private static string MakeSafeFileName(
            string value)
        {
            char[] invalid =
                Path.GetInvalidFileNameChars();


            return new string(
                value.Select(
                        c => invalid.Contains(c)
                            ? '_'
                            : c)
                    .ToArray());
        }
    }
}
