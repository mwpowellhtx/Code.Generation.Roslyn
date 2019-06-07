using System;
using System.Collections.Generic;

namespace Code.Generation.Roslyn
{
    // TODO: TBD: does not sound like much, but might be interesting as a smaller package...
    /// <summary>
    /// Static Enumerations helpers class.
    /// </summary>
    internal static class Enums
    {
        /// <summary>
        /// Verifies that <typeparamref name="T"/> is in fact an <see cref="Type.IsEnum"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private static Type Verify<T>()
            where T : struct
        {
            var type = typeof(T);

            if (!type.IsEnum)
            {
                throw new InvalidOperationException($"Type `{type.FullName}´ is not an Enum.");
            }

            return type;
        }

        /// <summary>
        /// Returns the <see cref="Enum"/> <typeparamref name="T"/> values.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> GetValues<T>()
            where T : struct
        {

            foreach (T x in Enum.GetValues(Verify<T>()))
            {
                yield return x;
            }
        }
    }
}
