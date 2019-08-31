using System;
using System.Collections.Generic;

namespace Code.Generation.Roslyn
{
    /// <summary>
    /// Represents an <see cref="IRegistrySet{T}"/> with additional <see cref="PurgeWhere"/>
    /// capability.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <inheritdoc />
    public interface IPurgingRegistrySet<T> : IRegistrySet<T>
    {
        /// <summary>
        /// This is slightly stronger in nature than <see cref="SortedSet{T}.RemoveWhere"/>, in
        /// which we expect more than just the collection elements to be removed. Maybe resources
        /// would be cleaned up, dealt with, files otherwise handled, that sort of thing.
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        int PurgeWhere(Predicate<T> predicate);
    }
}
