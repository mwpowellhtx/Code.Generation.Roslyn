using System.Collections.Generic;

namespace Code.Generation.Roslyn
{
    /// <summary>
    /// Represents a Set of Registry concerns.
    /// </summary>
    public interface IRegistrySet
    {
        /// <summary>
        /// Gets or Sets the Output Directory.
        /// </summary>
        string OutputDirectory { get; set; }
    }

    /// <inheritdoc cref="ISet{T}"/>
    public interface IRegistrySet<T> : ISet<T>, IRegistrySet
    {
    }
}
