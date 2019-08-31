using System.Collections.Generic;
using System.Linq;

namespace Code.Generation.Roslyn
{
    internal static class CollectionExtensionMethods
    {
        public static IReadOnlyCollection<T> ToReadOnlyCollection<T>(this IEnumerable<T> values) => values.ToArray();
    }
}
