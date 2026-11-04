using System;
using System.IO;
using System.Linq;

namespace ExhibitionCenter.Services
{
    public static class DeviceMediaStorage
    {
        private static string RootDirectory
        {
            get
            {
                string path =
                    Path.Combine(
                        Environment.GetFolderPath(
                            Environment.SpecialFolder.LocalApplicationData),
                        "ExhibitionCenter",
                        "DeviceMedia");


                Directory.CreateDirectory(path);


                return path;
            }
        }


        private static string ImageDirectory
        {
            get
            {
                string path =
                    Path.Combine(
                        RootDirectory,
                        "Images");


                Directory.CreateDirectory(path);


                return path;
            }
        }


        private static string VideoDirectory
        {
            get
            {
                string path =
                    Path.Combine(
                        RootDirectory,
                        "Videos");


                Directory.CreateDirectory(path);


                return path;
            }
        }


        public static string StoreImage(
            string id,
            string sourcePath)
        {
            return Store(
                id,
                sourcePath,
                ImageDirectory);
        }


        public static string StoreVideo(
            string id,
            string sourcePath)
        {
            return Store(
                id,
                sourcePath,
                VideoDirectory);
        }


        public static void RemoveImage(
            string id)
        {
            Remove(
                id,
                ImageDirectory);
        }


        public static void RemoveVideo(
            string id)
        {
            Remove(
                id,
                VideoDirectory);
        }


        private static string Store(
            string id,
            string sourcePath,
            string destinationDirectory)
        {
            if (string.IsNullOrWhiteSpace(sourcePath))
            {
                return string.Empty;
            }


            if (!File.Exists(sourcePath))
            {
                return string.Empty;
            }


            string sourceFull =
                Path.GetFullPath(sourcePath);


            string destinationRoot =
                Path.GetFullPath(
                    destinationDirectory);


            if (sourceFull.StartsWith(
                    destinationRoot,
                    StringComparison.OrdinalIgnoreCase))
            {
                return sourceFull;
            }


            string safeId =
                MakeSafeFileName(id);


            string extension =
                Path.GetExtension(sourcePath)
                    .ToLowerInvariant();


            string destination =
                Path.Combine(
                    destinationDirectory,
                    $"{safeId}_{DateTime.Now:yyyyMMddHHmmssfff}{extension}");


            File.Copy(
                sourcePath,
                destination,
                true);


            foreach (string oldFile in
                     Directory.GetFiles(
                         destinationDirectory,
                         $"{safeId}_*.*"))
            {
                if (string.Equals(
                        Path.GetFullPath(oldFile),
                        Path.GetFullPath(destination),
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }


                try
                {
                    File.Delete(oldFile);
                }
                catch
                {
                }
            }


            return destination;
        }


        private static void Remove(
            string id,
            string directory)
        {
            string safeId =
                MakeSafeFileName(id);


            foreach (string file in
                     Directory.GetFiles(
                         directory,
                         $"{safeId}_*.*"))
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
