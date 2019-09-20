using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Code.Generation.Roslyn
{
    using static Path;

    /// <summary>
    /// We determined that &quot;RemoveWhere&quot; is a bit too aggressive where the
    /// desired behavior is concerned. So we have identified an opportunity for a predicated
    /// <see cref="PurgeWhere"/> to take place.
    /// </summary>
    /// <inheritdoc />
    /// <see cref="GeneratedSyntaxTreeDescriptor"/>
    /// <see cref="GeneratedSyntaxTreeDescriptorComparer"/>
    public class GeneratedSyntaxTreeRegistry : PurgingSyntaxTreeRegistry<
        GeneratedSyntaxTreeDescriptor, GeneratedSyntaxTreeDescriptorComparer>
    {
        /// <summary>
        /// Gets the GeneratedSourceBundles for use by callers.
        /// </summary>
        public IDictionary<string, string[]> GeneratedSourceBundles
            => this.ToDictionary(x => x.SourceFilePath
                , x => x.GeneratedAssetKeys.Select(y => y.RenderGeneratedFileName())
                    .Select(y => Combine(OutputDirectory, y)).ToArray());

        /// <summary>
        /// Gets a Default Comparer.
        /// </summary>
        private static GeneratedSyntaxTreeDescriptorComparer DefaultComparer
            => GeneratedSyntaxTreeDescriptorComparer.Comparer;

        /// <summary>
        /// Default Public Constructor.
        /// </summary>
        /// <inheritdoc />
        public GeneratedSyntaxTreeRegistry()
            : base(DefaultComparer)
        {
        }

        /// <summary>
        /// Public Constructor.
        /// </summary>
        /// <param name="descriptors"></param>
        /// <inheritdoc />
        public GeneratedSyntaxTreeRegistry(IEnumerable<GeneratedSyntaxTreeDescriptor> descriptors)
            : base(descriptors, DefaultComparer)
        {
        }

        /// <summary>
        /// Responds when Purge of Generated Assets is occurring.
        /// </summary>
        /// <param name="descriptor"></param>
        protected virtual void OnPurgeGeneratedAsset(GeneratedSyntaxTreeDescriptor descriptor, string path)
        {
            // TODO: TBD: do we really need to check Exists/Delete so aggressively here?
            if (!File.Exists(path))
            {
                return;
            }

            File.Delete(path);
        }

        /// <summary>
        /// Responds when Purge of Generated Assets is occurring.
        /// </summary>
        /// <param name="descriptor"></param>
        /// <see cref="OnPurgeGeneratedAsset(GeneratedSyntaxTreeDescriptor, string)"/>
        protected virtual void OnPurgeGeneratedAsset(GeneratedSyntaxTreeDescriptor descriptor)
        {
            foreach (var y in descriptor.GeneratedAssetKeys.Select(x => x.RenderGeneratedFileName()))
            {
                OnPurgeGeneratedAsset(descriptor, Combine(OutputDirectory, y));
            }
        }

        /// <inheritdoc />
        /// <see cref="OnPurgeGeneratedAsset(GeneratedSyntaxTreeDescriptor)"/>
        public override int PurgeWhere(Predicate<GeneratedSyntaxTreeDescriptor> predicate)
        {
            // TODO: TBD: really need an EligibleSyntaxTreeRegistry... which is the in-memory version of the Generated ...
            predicate = predicate ?? (_ => true);

            foreach (var y in this.Where(x => predicate(x)).ToArray())
            {
                OnPurgeGeneratedAsset(y);
            }

            return base.PurgeWhere(predicate);
        }
    }
}
