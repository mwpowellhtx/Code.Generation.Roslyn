using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Code.Generation.Roslyn
{
    using static Path;

    // "RemoveWhere" is a bit too aggressive, or we need to add some flags that help to control the level of removal we expect.
    public class GeneratedSyntaxTreeRegistry : PurgingSyntaxTreeRegistry<GeneratedSyntaxTreeDescriptor, GeneratedSyntaxTreeDescriptorComparer>
    {
        internal IDictionary<string, string[]> GeneratedSourceBundles
            => this.ToDictionary(x => x.SourceFilePath
                , x => x.GeneratedAssetKeys.Select(y => $"{y:D}.g.cs")
                    .Select(y => Combine(OutputDirectory, y)).ToArray());

        /// <summary>
        /// Gets a Default Comparer.
        /// </summary>
        private static GeneratedSyntaxTreeDescriptorComparer DefaultComparer => GeneratedSyntaxTreeDescriptorComparer.Comparer;

        /// <summary>
        /// Default Public Constructor.
        /// </summary>
        /// <inheritdoc />
        public GeneratedSyntaxTreeRegistry() : base(DefaultComparer) { }

        // ReSharper disable once RedundantEmptyObjectOrCollectionInitializer
        /// <summary>
        /// Internal Constructor.
        /// </summary>
        /// <param name="descriptors"></param>
        /// <inheritdoc />
        internal GeneratedSyntaxTreeRegistry(IEnumerable<GeneratedSyntaxTreeDescriptor> descriptors) : base(descriptors, DefaultComparer) { }

        /// <inheritdoc />
        public override int PurgeWhere(Predicate<GeneratedSyntaxTreeDescriptor> predicate)
        {
            // TODO: TBD: really need an EligibleSyntaxTreeRegistry... which is the in-memory version of the Generated ...
            predicate = predicate ?? (_ => true);

            // TODO: TBD: do we really need to check Exists/Delete so aggressively here?
            foreach (var y in this.Where(x => predicate(x)))
            {
                // TODO: TBD: this bit probably ought to go with the descriptor anyways ...
                foreach (var path in y.GeneratedAssetKeys.Select(z => Combine(OutputDirectory, $"{z:D}.g.cs")))
                {
                    if (!File.Exists(path))
                    {
                        continue;
                    }

                    File.Delete(path);
                }
            }

            return base.PurgeWhere(predicate);
        }
    }
}
