using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Code.Generation.Roslyn
{
    using static Path;
    using static StringComparison;

    public class GeneratedSyntaxTreeRegistry : SortedSet<GeneratedSyntaxTreeDescriptor>, IRegistrySet<GeneratedSyntaxTreeDescriptor>
    {
        // TODO: TBD: ignore this one for JSON purposes...
        /// <summary>
        /// Gets or Sets the OutputDirectory.
        /// </summary>
        public string OutputDirectory { get; set; }

        private class InputSyntaxTreeFileDescriptorComparer : IComparer<GeneratedSyntaxTreeDescriptor>
        {
            public int Compare(GeneratedSyntaxTreeDescriptor x, GeneratedSyntaxTreeDescriptor y)
            {
                const int lt = -1, gt = 1;

                return x == null && y == null
                    ? lt
                    : x != null && y == null
                        ? gt
                        : x == null
                            ? lt
                            : string.Compare(x.SourceFilePath, y.SourceFilePath, InvariantCultureIgnoreCase);
            }
        }

        internal IDictionary<string, string[]> GeneratedSourceBundles
            => this.ToDictionary(x => x.SourceFilePath
                , x => x.GeneratedAssetKeys.Select(y => $"{y:D}.g.cs")
                    .Select(y => Combine(OutputDirectory, y)).ToArray());

        public GeneratedSyntaxTreeRegistry() : this(Array.Empty<GeneratedSyntaxTreeDescriptor>())
        {
        }

        // ReSharper disable once RedundantEmptyObjectOrCollectionInitializer
        internal GeneratedSyntaxTreeRegistry(IEnumerable<GeneratedSyntaxTreeDescriptor> descriptors)
            : base(descriptors, new InputSyntaxTreeFileDescriptorComparer { })
        {
            // TODO: TBD: why did we elect to purge the descriptors initially?
            RemoveWhere(null);
        }

        /// <summary>
        /// Invoke this method in order to Purge the Set of any underlying generated assets.
        /// This includes all actual generated files.
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        /// <inheritdoc cref="SortedSet{T}.RemoveWhere"/>
        public new int RemoveWhere(Predicate<GeneratedSyntaxTreeDescriptor> predicate)
        {
            predicate = predicate ?? (_ => true);

            foreach (var y in this.Where(x => predicate(x)))
            {
                foreach (var path in y.GeneratedAssets.Select(z => Combine(OutputDirectory, $"{z}.g.cs")))
                {
                    if (!File.Exists(path))
                    {
                        continue;
                    }

                    File.Delete(path);
                }
            }

            return base.RemoveWhere(predicate);
        }
    }
}
