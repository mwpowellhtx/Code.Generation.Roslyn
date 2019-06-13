using System.IO;

namespace Code.Generation.Roslyn
{
    internal static class RegistryExtensionMethods
    {
        public static TRegistry EnsureOutputDirectoryExists<TRegistry>(this TRegistry registry)
            where TRegistry : IRegistrySet
        {
            if (!Directory.Exists(registry.OutputDirectory))
            {
                Directory.CreateDirectory(registry.OutputDirectory);
            }

            return registry;
        }
    }
}
