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
        public AssemblyDescriptorRegistry() : this(Array.Empty<AssemblyDescriptor>())
        {
        }

        /// <summary>
        /// Internal Constructor.
        /// </summary>
        /// <param name="descriptors"></param>
        /// <inheritdoc />
        internal AssemblyDescriptorRegistry(IEnumerable<AssemblyDescriptor> descriptors) : base(descriptors, DefaultComparer) { }
    }
}
