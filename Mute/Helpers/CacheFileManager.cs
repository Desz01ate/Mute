using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Mute.Helpers
{
    public static class CacheFileManager
    {
        private const string CACHE_DIR = "cache";
        static CacheFileManager()
        {
            if (!Directory.Exists(CACHE_DIR))
            {
                Directory.CreateDirectory(CACHE_DIR);
            }
        }
        public static bool IsFileExists(string fileName) => File.Exists(Path.Combine(CACHE_DIR, fileName));

        public static FileStream GetFileCache(string fileName)
        {
            if (IsFileExists(fileName))
            {
                return File.OpenRead(Path.Combine(CACHE_DIR, fileName));
            }
            return default;
        }
        public static bool TryGetFileCache(string fileName, out FileStream fileStream)
        {
            if (IsFileExists(fileName))
            {
                fileStream = File.OpenRead(Path.Combine(CACHE_DIR, fileName));
                return true;
            }
            fileStream = null;
            return false;
        }
        public static string SaveCache(string fileName, Stream stream)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException(nameof(fileName));
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            var path = Path.Combine(CACHE_DIR, fileName);
            using var fs = File.Create(path);
            stream.Seek(0, SeekOrigin.Begin);
            stream.CopyTo(fs);
            return path;
        }
        public static string SaveCache(string fileName, byte[] data)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException(nameof(fileName));
            if (data == null || data.Length == 0)
                throw new ArgumentNullException(nameof(data));

            var path = Path.Combine(CACHE_DIR, fileName);
            using var fs = File.Create(path);
            fs.Write(data, 0, data.Length);
            return path;
        }
        public static string SaveCache(string fileName, string content)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException(nameof(fileName));

            var contentBytes = Encoding.UTF8.GetBytes(content);
            return SaveCache(fileName, contentBytes);
        }
        public static void ClearCache()
        {
            foreach (var file in Directory.EnumerateFiles(CACHE_DIR))
            {
                File.Delete(file);
            }
        }
        public static string GetCacheSize()
        {
            var totalSize = Math.Round(Directory.EnumerateFiles(CACHE_DIR).Sum(f => new FileInfo(f).Length) / 1024f / 1024f, 2);
            return $"{totalSize} MB";
        }

        public static string GetFullCachePath(string fileName)
        {
            if (!IsFileExists(fileName))
                throw new FileNotFoundException();
            return Path.GetFullPath(Path.Combine(CACHE_DIR, fileName));
        }

        public static string GetTempPath()
        {
            var guid = Guid.NewGuid() + ".tmp";
            return Path.GetFullPath(Path.Combine(CACHE_DIR, guid));
        }
    }
}
