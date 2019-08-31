using System;
using System.Collections.Generic;

namespace Code.Generation.Roslyn
{
    public class GeneratedSyntaxTreeDescriptor
    {
        /// <summary>
        /// &quot;&quot;
        /// </summary>
        public const string DefaultSourceFilePath = "";

        /// <summary>
        /// Gets whether IsRegenerated. Default is false, or rather, when loading
        /// the Registry for the first time during Compilation.
        /// </summary>
        internal bool IsRegenerated { get; private set; }

        /// <summary>
        /// Private Constructor.
        /// </summary>
        private GeneratedSyntaxTreeDescriptor()
        {
        }

        /// <summary>
        /// Creates a new Descriptor instance. A Null or Empty <paramref name="sourceFilePath"/>
        /// indicates Assembly Level Code Generation.
        /// </summary>
        /// <param name="sourceFilePath"></param>
        /// <returns></returns>
        public static GeneratedSyntaxTreeDescriptor Create(string sourceFilePath = DefaultSourceFilePath)
            => new GeneratedSyntaxTreeDescriptor
            {
                SourceFilePath = sourceFilePath,
                LastModifiedTimestamp = sourceFilePath.GetRegistryLastWriteTimeUtc(),
                IsRegenerated = true
            };

        /// <summary>
        /// Gets or Sets the Root SourceFilePath triggering the Code Generation for the
        /// <see cref="GeneratedAssetKeys"/> collection. A Null or Empty Path means Code
        /// Generation occurred at the Assembly Level.
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
