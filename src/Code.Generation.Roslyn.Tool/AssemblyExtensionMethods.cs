using System;
using System.Reflection;

namespace Code.Generation.Roslyn
{
    // TODO: TBD: might be worth introducing some helpful assembly extension methods...
    internal static class AssemblyExtensionMethods
    {
        public static string GetAssemblyInformationalVersion<T>(this T _)
            => typeof(T).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                   ?.InformationalVersion ?? string.Empty;

        public static Version GetAssemblyVersion<T>(this T _) => typeof(T).Assembly.GetName().Version;
    }
}
