using System;

namespace Code.Generation.Roslyn
{
    using static String;

    /// <summary>
    /// Strings static helpers class.
    /// </summary>
    internal static class Strings
    {
        /// <summary>
        /// Returns whether <paramref name="s"/> is Not <see cref="IsNullOrEmpty"/>.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsNotNullOrEmpty(string s) => !IsNullOrEmpty(s);
    }
}
