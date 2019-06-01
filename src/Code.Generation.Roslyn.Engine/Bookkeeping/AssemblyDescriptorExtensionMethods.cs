using System.Collections.Generic;

namespace Code.Generation.Roslyn
{
    internal static class AssemblyDescriptorExtensionMethods
    {
        public static AssemblyDescriptorRegistry ToRegistry(this IEnumerable<AssemblyDescriptor> descriptors)
            => new AssemblyDescriptorRegistry(descriptors);
    }
}
