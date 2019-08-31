using System.IO;
using System.Linq;

namespace Code.Generation.Roslyn
{
    internal static class PathExtensionMethods
    {
        public static string CombinePaths(this string basePath, params string[] otherPaths)
            => Path.Combine(new[] {basePath}.Concat(otherPaths).ToArray());
    }
}
