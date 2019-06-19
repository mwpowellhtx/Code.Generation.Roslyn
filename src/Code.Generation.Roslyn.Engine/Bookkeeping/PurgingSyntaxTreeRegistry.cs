using System;
using System.Collections.Generic;

namespace Code.Generation.Roslyn
{
    /// <inheritdoc cref="IPurgingRegistrySet{T}" />
    public class PurgingSyntaxTreeRegistry<T, TComparer> : SortedSet<T>, IPurgingRegistrySet<T>
        where TComparer : IComparer<T>
    {
        // TODO: TBD: ignore this one for JSON purposes...
        /// <summary>
        /// Gets or Sets the OutputDirectory.
        /// </summary>
        public string OutputDirectory { get; set; }

        /// <summary>
        /// Default Protected Constructor.
        /// </summary>
        /// <inheritdoc />
        protected PurgingSyntaxTreeRegistry(TComparer comparer) : this(Array.Empty<T>(), comparer) { }

        /// <summary>
        /// Protected Constructor.
        /// </summary>
        /// <param name="descriptors"></param>
        /// <inheritdoc />
        protected PurgingSyntaxTreeRegistry(IEnumerable<T> descriptors, TComparer comparer)
            : base(descriptors, comparer) { }

        // ReSharper disable once RedundantEmptyObjectOrCollectionInitializer
        /// <summary>
        /// Protected Constructor.
        /// </summary>
        /// <param name="registrySet"></param>
        /// <inheritdoc />
        internal PurgingSyntaxTreeRegistry(IRegistrySet<T> registrySet, TComparer comparer)
            : this((IEnumerable<T>) registrySet, comparer)
        {
            OutputDirectory = registrySet.OutputDirectory;
        }

        // TODO: TBD: this may not be necessary after all if we introduce a PurgeWhere ...
        protected virtual int BaseRemoveWhere(Predicate<T> predicate) => RemoveWhere(predicate);

        /// <summary>
        /// Purging is necessarily stronger than <see cref="SortedSet{T}.RemoveWhere"/>. Only
        /// call this method when you are ready to do the stronger response than simply removing
        /// elements from the collection itself.
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        /// <inheritdoc />
        public virtual int PurgeWhere(Predicate<T> predicate) => BaseRemoveWhere(predicate);
    }
}
