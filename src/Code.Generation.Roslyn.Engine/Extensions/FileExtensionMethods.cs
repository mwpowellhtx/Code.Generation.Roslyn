using System;
using System.IO;

namespace Code.Generation.Roslyn
{
    using static String;

    internal static class FileExtensionMethods
    {
        public static DateTime? GetLastWriteTimeUtc(this string path)
            => IsNullOrEmpty(path) || !File.Exists(path)
                ? (DateTime?) null
                : File.GetLastWriteTimeUtc(path);
    }
}
