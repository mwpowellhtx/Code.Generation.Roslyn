namespace Code.Generation.Roslyn.Integration
{
    internal static class Resources
    {
        /// <summary>
        /// Combines the <paramref name="names"/> in an Embedded Resource Path.
        /// </summary>
        /// <param name="names"></param>
        /// <returns></returns>
        public static string Combine(params string[] names) => string.Join(".", names);
    }
}
