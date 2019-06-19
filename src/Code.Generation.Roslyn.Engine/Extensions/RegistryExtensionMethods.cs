using System;
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

        // TODO: TBD: for now, assuming generating CSharp ...
        public static string MakeRelativeSourcePath<TRegistry>(this TRegistry registry, Guid generatedId)
            where TRegistry : IRegistrySet
            => Path.Combine(registry.EnsureOutputDirectoryExists().OutputDirectory, $"{generatedId:D}.g.cs");
    }
}
