using System;
using System.Collections.Generic;

namespace Code.Generation.Roslyn
{
    using static StringComparison;

    /// <summary>
    /// <see cref="GeneratedSyntaxTreeDescriptor"/> <see cref="IComparer{T}"/> asset.
    /// </summary>
    /// <inheritdoc />
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

        /// <summary>
        /// Gets an Internal Comparer instance.
        /// </summary>
        public static GeneratedSyntaxTreeDescriptorComparer Comparer => new GeneratedSyntaxTreeDescriptorComparer();

        /// <summary>
        /// Compares <see cref="string"/> <paramref name="x"/> and <paramref name="y"/>.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        /// <remarks>Allows for better support of Null
        /// <see cref="GeneratedSyntaxTreeDescriptor.SourceFilePath"/> property values.</remarks>
        private static int Compare(string x, string y)
            => x == null && y == null
                ? LessThan
                : x != null && y == null
                    ? GreaterThan
                    : x == null
                        ? LessThan
                        : string.Compare(x, y, InvariantCultureIgnoreCase);

        /// <summary>
        /// Compares <paramref name="x"/> with <paramref name="y"/>.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int Compare(GeneratedSyntaxTreeDescriptor x, GeneratedSyntaxTreeDescriptor y)
            => x == null && y == null
                ? LessThan
                : x != null && y == null
                    ? GreaterThan
                    : x == null
                        ? LessThan
                        : Compare(x.SourceFilePath, y.SourceFilePath);
    }
}
