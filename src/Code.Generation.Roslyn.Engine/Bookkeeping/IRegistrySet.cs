using System.Collections.Generic;

namespace Code.Generation.Roslyn
{
    public interface IRegistrySet
    {
        string OutputDirectory { get; set; }
    }

    /// <inheritdoc cref="ISet{T}"/>
    public interface IRegistrySet<T> : ISet<T>, IRegistrySet
    {
    }
}
