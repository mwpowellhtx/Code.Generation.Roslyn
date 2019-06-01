using System.Collections.Generic;

namespace Code.Generation.Roslyn
{
    /// <inheritdoc />
    public interface IRegistrySet<T> : ISet<T>
    {
        string OutputDirectory { get; set; }
    }
}
