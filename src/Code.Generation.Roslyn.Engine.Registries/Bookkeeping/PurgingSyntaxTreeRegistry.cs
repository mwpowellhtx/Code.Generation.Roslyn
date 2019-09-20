using System;
using System.Collections.Generic;
using System.Linq;

namespace Code.Generation.Roslyn
{
    /// <inheritdoc cref="IPurgingRegistrySet{T}" />
    public class PurgingSyntaxTreeRegistry<T, TComparer> : SortedSet<T>, IPurgingRegistrySet<T>
        where TComparer : IComparer<T>
    {
        // TODO: TBD: ignore this one for JSON purposes...
        /// <inheritdoc />
        public string OutputDirectory { get; set; }

        // TODO: TBD: if memory serves, the last time we tried this, we had some problems telling the difference between the base class `SortedSet´ and `what else´ was important about a JSON serialization...
        // TODO: TBD: or along these lines at any rate... as such, we might need a DTO of sorts in order to bridge the gap between the registry set and the actual JSON serialization whose only purpose is to host the actual List, plus whatever JSON-y important bits we need to maintain ...
        // TODO: TBD: we might also consider a custom JSON converter class that takes the registry set and knows how to deal with it...
        /// <summary>
        /// Gets or Sets the Items. This is not really intended for anything else other than
        /// for JSON serialization purposes. One is to use the routine Add methods in order
        /// to do business through this Registry interface.
        /// </summary>
        /// <remarks>Note, this property is also not anything we want to expose via the
        /// <see cref="IPurgingRegistrySet{T}"/> interface.</remarks>
        public List<T> Items
        {
            get => this.ToList();
            set
            {
                // Clear out previously existing Items prior to re-Adding new ones wholesale.
                Clear();
                // TODO: TBD: can probably rethink this part in favor of an enumerable getter/setter ...
                void Add(T item) => this.Add(item);
                (value ?? new List<T>()).ForEach(Add);
            }
        }

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
            : this((IEnumerable<T>)registrySet, comparer)
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
