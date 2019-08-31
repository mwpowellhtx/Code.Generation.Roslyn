using System;
using System.Collections.Generic;

namespace Code.Generation.Roslyn
{
    /// <inheritdoc cref="SortedSet{T}"/>
    public class AssemblyDescriptorRegistry : PurgingSyntaxTreeRegistry<AssemblyDescriptor, AssemblyDescriptorComparer>
    {
        private static AssemblyDescriptorComparer DefaultComparer => AssemblyDescriptorComparer.Comparer;

        /// <summary>
        /// Default Constructor.
        /// </summary>
        /// <inheritdoc />
        public AssemblyDescriptorRegistry()
            : this(Array.Empty<AssemblyDescriptor>())
        {
        }

        /// <summary>
        /// Internal Constructor.
        /// </summary>
        /// <param name="descriptors"></param>
        /// <inheritdoc />
        internal AssemblyDescriptorRegistry(IEnumerable<AssemblyDescriptor> descriptors)
            : base(descriptors, DefaultComparer)
        {
        }

        /// <summary>
        /// Returns an implicitly converted <paramref name="dto"/> to
        /// <see cref="AssemblyDescriptorRegistry"/>.
        /// </summary>
        /// <param name="dto"></param>
        public static implicit operator AssemblyDescriptorRegistry(AssemblyDescriptorRegistryTransferObject dto)
        {
            var registry = new AssemblyDescriptorRegistry();
            // With a little help from an LF making it List ForEach friendly.
            void Add(AssemblyDescriptor x) => registry.Add(x);
            dto.Descriptors.ForEach(Add);
            return registry;
        }
    }
}
