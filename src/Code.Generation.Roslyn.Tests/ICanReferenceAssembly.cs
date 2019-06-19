using System.Reflection;
using System.IO;

namespace Code.Generation.Roslyn
{
    /// <summary>
    /// Whether we are talking about Metadata References or actual <see cref="Assembly"/>
    /// References, we want to furnish a consistent interface regardless. That will make
    /// it easier to extend into unit testing later on.
    /// </summary>
    /// <typeparam name="TParent"></typeparam>
    public interface ICanReferenceAssembly<out TParent>
    {
        /// <summary>
        /// Adds References to the <paramref name="assetRelativeNames"/>, usually Assembly
        /// Dynamic Link Libraries (DLLs), but may be dotnet Executable (EXE) as well. Uses
        /// the <typeparamref name="T"/> host <see cref="Assembly.Location"/>
        /// <see cref="Path.GetDirectoryName(string)"/> as the base directory.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetRelativeNames"></param>
        /// <returns></returns>
        /// <see cref="!:https://stackoverflow.com/questions/23907305#47196516">Roslyn has
        /// no reference to System.Runtime</see>
        /// <remarks>This method is provided as a workaround to the references System.Runtime issue.</remarks>
        TParent AddTypeAssemblyLocationBasedReferences<T>(params string[] assetRelativeNames);

        /// <summary>
        /// Adds a Reference to the <typeparamref name="T"/> <see cref="Assembly.Location"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        TParent AddReferenceToTypeAssembly<T>();

        /// <summary>
        /// Adds a Reference to each of the <paramref name="paths"/>. Note, it is caller
        /// responsibility in this instance to ensure that the Paths are correct.
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        TParent AddReferencesByPaths(params string[] paths);
    }
}
