using System;
using System.IO;

// TODO: TBD: in addition to refactoring some things, we decided to refactor these bits to the targets instead...
namespace Code.Generation.Roslyn
{
    using Validation;

    internal static class RegistryExtensionMethods
    {
        private static TRegistry EnsureOutputDirectoryExists<TRegistry>(this TRegistry registry)
            where TRegistry : class, IRegistrySet
        {
            Requires.NotNull(registry, nameof(registry));
            Requires.NotNull(registry.OutputDirectory, nameof(registry.OutputDirectory));

            if (!Directory.Exists(registry.OutputDirectory))
            {
                Directory.CreateDirectory(registry.OutputDirectory);
            }

            return registry;
        }

        // TODO: TBD: for now, assuming generating CSharp ...
        public static string MakeRelativeSourcePath<TRegistry>(this TRegistry registry, Guid generatedId)
            where TRegistry : class, IRegistrySet
            => Path.Combine(registry.EnsureOutputDirectoryExists().OutputDirectory, $"{generatedId:D}.g.cs");
    }
}
