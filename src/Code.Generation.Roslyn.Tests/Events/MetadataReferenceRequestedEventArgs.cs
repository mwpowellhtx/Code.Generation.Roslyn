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
    /// <inheritdoc cref="ICanReferenceAssembly{T}"/>
    public class ResolveMetadataReferencesEventArgs
        : EventArgs
            , ICanReferenceAssembly<ResolveMetadataReferencesEventArgs>
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

        /// <inheritdoc />
        public ResolveMetadataReferencesEventArgs AddTypeAssemblyLocationBasedReferences<T>(params string[] assetRelativeNames)
        {
            var typeAssemblyLocation = GetTypeAssembly<T>().Location;
            var locationDirectoryName = GetDirectoryName(typeAssemblyLocation);
            return assetRelativeNames.Select(x => Combine(locationDirectoryName, x)).Aggregate(this, AddReferenceByPath);
        }

        /// <inheritdoc />
        public ResolveMetadataReferencesEventArgs AddReferenceToTypeAssembly<T>() => AddReferenceByPath(this, GetTypeAssembly<T>().Location);

        /// <inheritdoc />
        public ResolveMetadataReferencesEventArgs AddReferencesByPaths(params string[] paths) => paths.Aggregate(this, AddReferenceByPath);
        // ReSharper restore UnusedMember.Global
    }
}
