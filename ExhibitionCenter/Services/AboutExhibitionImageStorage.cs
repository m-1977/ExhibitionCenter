using System;
using System.IO;

namespace ExhibitionCenter.Services
{
    public static class AboutExhibitionImageStorage
    {
        private static string Folder =>
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "ExhibitionCenter",
                "AboutExhibition");


        public static string Store(
            string sourcePath)
        {
            if (string.IsNullOrWhiteSpace(
                    sourcePath) ||
                !File.Exists(sourcePath))
            {
                return string.Empty;
            }


            Directory.CreateDirectory(
                Folder);


            string extension =
                Path.GetExtension(
                    sourcePath);


            string destination =
                Path.Combine(
                    Folder,
                    "about-image" +
                    extension);


            string sourceFull =
                Path.GetFullPath(
                    sourcePath);


            string destinationFull =
                Path.GetFullPath(
                    destination);


            if (string.Equals(
                    sourceFull,
                    destinationFull,
                    StringComparison.OrdinalIgnoreCase))
            {
                return destinationFull;
            }


            Remove();


            File.Copy(
                sourcePath,
                destination,
                true);


            return destination;
        }


        public static void Remove()
        {
            if (!Directory.Exists(
                    Folder))
            {
                return;
            }


            foreach (string file
                     in Directory.GetFiles(
                         Folder,
                         "about-image.*"))
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
    }
}
