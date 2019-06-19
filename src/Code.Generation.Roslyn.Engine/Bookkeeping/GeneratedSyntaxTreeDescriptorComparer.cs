using System;
using System.Collections.Generic;

namespace Code.Generation.Roslyn
{
    using static StringComparison;

    public class GeneratedSyntaxTreeDescriptorComparer : IComparer<GeneratedSyntaxTreeDescriptor>
    {
        /// <summary>
        /// -1
        /// </summary>
        private const int LessThan = -1;

        /// <summary>
        /// 1
        /// </summary>
        private const int GreaterThan = 1;

        /// <summary>
        /// Internal Constructor.
        /// </summary>
        private GeneratedSyntaxTreeDescriptorComparer() { }

        internal static GeneratedSyntaxTreeDescriptorComparer Comparer => new GeneratedSyntaxTreeDescriptorComparer();

        public int Compare(GeneratedSyntaxTreeDescriptor x, GeneratedSyntaxTreeDescriptor y)
            => x == null && y == null
                ? LessThan
                : x != null && y == null
                    ? GreaterThan
                    : x == null
                        ? LessThan
                        : string.Compare(x.SourceFilePath, y.SourceFilePath, InvariantCultureIgnoreCase);
    }
}
