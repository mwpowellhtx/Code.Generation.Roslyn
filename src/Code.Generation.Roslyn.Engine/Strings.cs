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

        /// <summary>
        /// Returns the <paramref name="values"/> <see cref="string.Join(string,string[])"/>
        /// by the <paramref name="separator"/>. Since this is an extension method, the
        /// <paramref name="separator"/> anchors its invocation.
        /// </summary>
        /// <param name="separator"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static string Join(this string separator, params string[] values) => string.Join(separator, values);
    }
}
