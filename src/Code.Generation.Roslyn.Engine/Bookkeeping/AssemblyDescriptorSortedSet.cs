using System;
using System.Collections.Generic;

namespace Code.Generation.Roslyn
{
    /// <inheritdoc cref="SortedSet{T}"/>
    public  class AssemblyDescriptorRegistry : SortedSet<AssemblyDescriptor>, IRegistrySet<AssemblyDescriptor>
    {
        /// <inheritdoc />
        public string OutputDirectory { get; set; }

        private class AssemblyDescriptorComparer : IComparer<AssemblyDescriptor>
        {
            // ReSharper disable once EmptyConstructor
            internal AssemblyDescriptorComparer()
            {
            }

            public int Compare(AssemblyDescriptor x, AssemblyDescriptor y)
            {
                const int gt = 1, lt = -1;

                return x == null && y == null
                    ? lt
                    : x != null && y == null
                        ? gt
                        : x == null
                            ? lt
                            : string.Compare(x.AssemblyPath, y.AssemblyPath
                                , StringComparison.InvariantCultureIgnoreCase);
            }
        }

        public AssemblyDescriptorRegistry() : this(Array.Empty<AssemblyDescriptor>())
        {
        }

        internal AssemblyDescriptorRegistry(IEnumerable<AssemblyDescriptor> descriptors)
            // ReSharper disable once RedundantEmptyObjectOrCollectionInitializer
            : base(descriptors, new AssemblyDescriptorComparer { })
        {
        }
    }
}
