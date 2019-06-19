using System;
using System.IO;

namespace Code.Generation.Roslyn
{
    internal static class FileExtensionMethods
    {
        public static DateTime? GetLastWriteTimeUtc(this string path)
            => File.Exists(path) ? File.GetLastWriteTimeUtc(path) : (DateTime?) null;
    }
}
