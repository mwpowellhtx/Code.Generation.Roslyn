using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Code.Generation.Roslyn
{
    public class GeneratedSyntaxTreeDescriptor
    {
        /// <summary>
        /// Gets whether IsRegenerated. Default is false, or rather, when loading
        /// the Registry for the first time during Compilation.
        /// </summary>
        internal bool IsRegenerated { get; private set; }

        internal static GeneratedSyntaxTreeDescriptor Create(string sourceFilePath)
            => new GeneratedSyntaxTreeDescriptor
            {
                SourceFilePath = sourceFilePath,
                LastModifiedTimestamp = File.GetLastWriteTimeUtc(sourceFilePath),
                IsRegenerated = true
            };

        /// <summary>
        /// Gets or Sets the Root SourceFilePath triggering the Code Generation for the
        /// <see cref="GeneratedAssets"/> collection.
        /// </summary>
        public string SourceFilePath { get; set; }

        /// <summary>
        /// Gets or Sets the <see cref="SourceFilePath"/> LastModifiedTimestamp in terms
        /// of Universal Coordinated Time (UTC).
        /// </summary>
        /// <see cref="!:https://en.wikipedia.org/wiki/Coordinated_Universal_Time"/>
        public DateTime? LastModifiedTimestamp { get; set; }

        // ReSharper disable once RedundantEmptyObjectOrCollectionInitializer
        /// <summary>
        /// Gets or Sets a <see cref="List{T}"/> of GeneratedAssets by <see cref="Guid"/> key. All
        /// further bookkeeping operations all derive a full path based on the <see cref="Guid"/>.
        /// </summary>
        public ICollection<Guid> GeneratedAssetKeys { get; set; } = new List<Guid> { };
    }
}
