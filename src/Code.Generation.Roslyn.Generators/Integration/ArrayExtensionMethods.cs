using System;
using System.Collections.Generic;

namespace Code.Generation.Roslyn.Generators.Integration
{
    /// <summary>
    /// Provides a set of <see cref="Array"/> extension methods.
    /// </summary>
    internal static class ArrayExtensionMethods
    {
        /// <summary>
        /// Returns the <paramref name="array"/> elements in terms of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <returns></returns>
        public static IEnumerable<T> ToArray<T>(this Array array)
        {
            foreach (T x in array)
            {
                yield return x;
            }
        }
    }
}
