using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Code.Generation.Roslyn
{
    using Microsoft.CodeAnalysis;
    using static Microsoft.CodeAnalysis.MetadataReference;
    using static Path;

    /// <summary>
    /// Use the <see cref="ResolveMetadataReferencesEventArgs"/> members in order to
    /// specify the requisite <see cref="MetadataReference"/> instances.
    /// </summary>
    /// <inheritdoc />
    public class ResolveMetadataReferencesEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the Solution involved in the <see cref="MetadataReference"/> Resolution.
        /// The instance is furnished strictly for informational purposes only and because a
        /// <see cref="Microsoft.CodeAnalysis.Solution"/> is required for the Resolution to occur
        /// in the first place.
        /// </summary>
        public Solution Solution { get; internal set; }

        /// <summary>
        /// Gets the Project involved in the <see cref="MetadataReference"/> Resolution.
        /// The instance is furnished strictly for informational purposes only and because a
        /// <see cref="Microsoft.CodeAnalysis.Project"/> is required for the Resolution to occur
        /// in the first place.
        /// </summary>
        public Project Project { get; internal set; }

        // ReSharper disable RedundantEmptyObjectOrCollectionInitializer
        public ICollection<MetadataReference> MetadataReferences { get; } = new List<MetadataReference> { };

        private ICollection<string> PrivateUnableToAdd { get; } = new List<string> { };
        // ReSharper restore RedundantEmptyObjectOrCollectionInitializer

        // ReSharper disable UnusedMember.Global
        /// <summary>
        /// Gets the set of Locations we were Unable to Add over the course of this Resolution.
        /// </summary>
        public IEnumerable<string> UnableToAddLocations => PrivateUnableToAdd;

        /// <summary>
        /// Internal Constructor.
        /// </summary>
        /// <inheritdoc />
        internal ResolveMetadataReferencesEventArgs()
        {
        }

        /// <summary>
        /// Adds an entry to <see cref="MetadataReferences"/> given <paramref name="path"/>.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        /// <remarks>We do this statically as we do in order for aggregation to work
        /// well over a potential range of specified reference paths.</remarks>
        private static ResolveMetadataReferencesEventArgs AddReferenceByPath(ResolveMetadataReferencesEventArgs e, string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    e.PrivateUnableToAdd.Add(path);
                }
                else
                {
                    e.MetadataReferences.Add(CreateFromFile(path));
                }
            }
            catch (Exception ex)
            {
                e.PrivateUnableToAdd.Add(path);
            }

            return e;
        }

        // TODO: TBD: is the dependency on IntrospectionExtensions (System.Runtime.dll) really necessary?
        /// <summary>
        /// Gets the <see cref="Assembly"/> based on the
        /// <see cref="IntrospectionExtensions.GetTypeInfo"/> for the <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private static Assembly GetTypeAssembly<T>() => typeof(T).GetTypeInfo().Assembly;

        /// <summary>
        /// Adds References to the <paramref name="assetRelativeNames"/>, usually Assembly
        /// Dynamic Link Libraries (DLLs), but may be dotnet Executable (EXE) as well. Uses
        /// the <typeparamref name="T"/> host <see cref="Assembly.Location"/>
        /// <see cref="GetDirectoryName(string)"/> as the base directory.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetRelativeNames"></param>
        /// <returns></returns>
        /// <see cref="!:https://stackoverflow.com/questions/23907305#47196516">Roslyn has no reference to System.Runtime</see>
        /// <remarks>This method is provided as a workaround to the references System.Runtime issue.</remarks>
        public ResolveMetadataReferencesEventArgs AddTypeAssemblyLocationBasedReferences<T>(params string[] assetRelativeNames)
        {
            var typeAssemblyLocation = GetTypeAssembly<T>().Location;
            var locationDirectoryName = GetDirectoryName(typeAssemblyLocation);
            return assetRelativeNames.Select(x => Combine(locationDirectoryName, x)).Aggregate(this, AddReferenceByPath);
        }

        /// <summary>
        /// Adds a Reference to the <typeparamref name="T"/> <see cref="GetTypeAssembly{T}"/>
        /// furnished <see cref="Assembly.Location"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public ResolveMetadataReferencesEventArgs AddReferenceToTypeAssembly<T>() => AddReferenceByPath(this, GetTypeAssembly<T>().Location);

        /// <summary>
        /// Adds a Reference to each of the <paramref name="paths"/>. Note, it is caller
        /// responsibility in this instance to ensure that the Paths are correct.
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        public ResolveMetadataReferencesEventArgs AddReferencesByPaths(params string[] paths) => paths.Aggregate(this, AddReferenceByPath);
        // ReSharper restore UnusedMember.Global
    }
}
