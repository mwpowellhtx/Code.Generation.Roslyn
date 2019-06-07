using System.Reflection;

namespace Code.Generation.Roslyn
{
    /// <summary>
    /// Callback that Loads the <see cref="Assembly"/> associated with the
    /// <paramref name="assemblyName"/>.
    /// </summary>
    /// <param name="assemblyName"></param>
    /// <returns></returns>
    public delegate Assembly LoadAssemblyCallback(AssemblyName assemblyName);
}
