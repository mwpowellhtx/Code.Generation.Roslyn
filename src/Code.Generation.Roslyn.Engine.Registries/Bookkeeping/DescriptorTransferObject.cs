using System.Collections.Generic;

namespace Code.Generation.Roslyn
{
    /// <summary>
    /// Represents a base set of <typeparamref name="TDescriptor"/> Registry
    /// Transfer Object assets.
    /// </summary>
    /// <typeparam name="TDescriptor"></typeparam>
    public abstract class DescriptorRegistryTransferObject<TDescriptor>
    {
        // ReSharper disable once RedundantEmptyObjectOrCollectionInitializer
        /// <summary>
        /// Gets or Sets the <see cref="List{T}"/> of Descriptors
        /// </summary>
        public virtual List<TDescriptor> Descriptors { get; set; } = new List<TDescriptor> { };
    }
}
