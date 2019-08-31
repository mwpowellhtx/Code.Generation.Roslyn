using System;
using System.IO;

namespace Code.Generation.Roslyn
{
    using static String;

    /// <summary>
    /// File Extension Methods for Internal use.
    /// </summary>
    public static class FileExtensionMethods
    {
        /// <summary>
        /// Gets the Last Write Time in terms of Universal Coordinated Time (Utc).
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static DateTime? GetRegistryLastWriteTimeUtc(this string path)
            => IsNullOrEmpty(path) || !File.Exists(path)
                ? (DateTime?)null
                : File.GetLastWriteTimeUtc(path);
    }
}
