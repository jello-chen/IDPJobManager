using SharpCompress.Readers;
using System.IO;
using System;
using SharpCompress.Archives.Rar;
using System.Linq;
using SharpCompress.Archives;
using IDPJobManager.Core.Utils;

namespace IDPJobManager.Web.Utils
{
    public class ZipUtil
    {
        public static bool UnRar(Stream stream, string destination, bool ExtractFullPath = true, bool Overwrite = true, bool PreserveFileTime = true)
        {
            try
            {
                destination = PrepareDirectory(destination);
                var options = new ExtractionOptions { ExtractFullPath = ExtractFullPath, Overwrite = Overwrite, PreserveFileTime = PreserveFileTime };
                using (var archive = RarArchive.Open(stream))
                {
                    foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory))
                    {
                        entry.WriteToDirectory(destination, options);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(ex.Message);
                return false;
            }
        }

        public static bool IsRar(Stream stream)
        {
            return RarArchive.IsRarFile(stream);
        }

        private static string PrepareDirectory(string destination)
        {
            if (Path.IsPathRooted(destination))
            {
                if (!Directory.Exists(destination))
                    Directory.CreateDirectory(destination);
                return destination;
            }
            else
            {
                var absoluteDestination = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, destination);
                if (!Directory.Exists(absoluteDestination))
                    Directory.CreateDirectory(absoluteDestination);
                return absoluteDestination;
            }
        }
    }
}
