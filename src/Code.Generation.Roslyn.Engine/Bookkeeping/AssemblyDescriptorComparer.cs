using System;
using System.Collections.Generic;

namespace Code.Generation.Roslyn
{
    using static StringComparison;

    public class AssemblyDescriptorComparer : IComparer<AssemblyDescriptor>
    {
        internal static AssemblyDescriptorComparer Comparer => new AssemblyDescriptorComparer();

        /// <summary>
        /// Private Constructor.
        /// </summary>
        private AssemblyDescriptorComparer() { }

        /// <summary>
        /// -1
        /// </summary>
        private const int LessThan = -1;

        /// <summary>
        /// 1
        /// </summary>
        private const int GreaterThan = 1;

        public int Compare(AssemblyDescriptor x, AssemblyDescriptor y)
            => x == null && y == null
                ? LessThan
                : x != null && y == null
                    ? GreaterThan
                    : x == null
                        ? LessThan
                        : string.Compare(x.AssemblyPath, y.AssemblyPath
                            , InvariantCultureIgnoreCase);
    }
}
