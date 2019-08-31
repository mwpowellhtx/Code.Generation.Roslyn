using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Code.Generation.Roslyn
{
    using Validation;
    using static Directory;
    using static Path;
    using static Resources;
    using static String;
    using static SearchOption;
    using static StringComparison;

    // TODO: TBD: might even call it Manager of the underlying Descriptor Set...
    public class AssemblyReferenceServiceManager : ServiceManager<AssemblyDescriptor
        , AssemblyDescriptorRegistry, AssemblyDescriptorRegistryTransferObject>
    {
        /// <summary>
        /// Currently &quot;.dll&quot;.
        /// </summary>
        /// <remarks>Exposed as Internal to allow for easier unit testing in isolated test cases.</remarks>
        /// <see cref="AllowedAssemblyExtensionDll"/>
        internal static HashSet<string> AllowedAssemblyExtensions { get; }
            = new HashSet<string>(StringComparer.OrdinalIgnoreCase) {AllowedAssemblyExtensionDll};

        /// <summary>
        /// Gets the list of AdditionalReferencePaths.
        /// </summary>
        internal IReadOnlyCollection<string> AdditionalReferencePaths { get; }

        /// <summary>
        /// Gets the Directory Paths in which to Search for Generator Assemblies.
        /// </summary>
        private IReadOnlyCollection<string> SearchPaths { get; }

        /// <summary>
        /// Internal Constructor.
        /// </summary>
        /// <param name="outputDirectory">The Output Directory.</param>
        /// <param name="registryFileName">The Registry File Name.</param>
        /// <param name="additionalReferencePaths">The Additional Reference Paths.</param>
        /// <param name="searchPaths">The Assembly Search Paths.</param>
        /// <inheritdoc />
        internal AssemblyReferenceServiceManager(string outputDirectory, string registryFileName
        , IReadOnlyCollection<string> additionalReferencePaths, IReadOnlyCollection<string> searchPaths)
            : base(outputDirectory, registryFileName
                // Not unlike the CompilationUnits counterpart, we leverage the implicit type conversions.
                , x => x, x => x)
        {
            Verify.Operation(additionalReferencePaths != null, FormatVerifyOperationMessage(nameof(additionalReferencePaths)));
            Verify.Operation(searchPaths != null, FormatVerifyOperationMessage(nameof(searchPaths)));

            AdditionalReferencePaths = additionalReferencePaths;
            SearchPaths = searchPaths;

            PrivateDependencyContext = new CompilationAssemblyResolverDependencyContext(additionalReferencePaths);
        }

        /// <summary>
        /// Gets the Last Written Timestamp when the Assemblies were updated.
        /// </summary>
        internal DateTime? AssembliesLastWrittenTimestamp
            => TryLoad(out var registrySet)
                ? registrySet.Select(x => File.GetLastWriteTimeUtc(x.AssemblyPath)).Max()
                : (DateTime?) null;

        /// <summary>
        /// Purge those <see cref="AssemblyDescriptor.AssemblyPath"/> which Do Not Exist.
        /// </summary>
        /// <returns></returns>
        internal AssemblyReferenceServiceManager PurgeNotExists()
        {
            bool NotExists(string path) => !File.Exists(path);
            RegistrySet?.RemoveWhere(x => NotExists(x.AssemblyPath));
            TrySave();
            return this;
        }

        internal static string GetMatchingReferenceAssemblyPath(IReadOnlyCollection<string> paths,
            AssemblyName assemblyName)
            => paths.FirstOrDefault(
                x => GetFileNameWithoutExtension(x)
                         ?.Equals(assemblyName.Name, OrdinalIgnoreCase) ?? false
            );

        internal static string GetSearchMatchingAssemblyPath(IReadOnlyCollection<string> paths, AssemblyName assemblyName)
            => paths.SelectMany(
                    x => EnumerateFiles(x, $"{assemblyName.Name}{AllowedAssemblyExtensionDll}", TopDirectoryOnly))
                .FirstOrDefault(x => AllowedAssemblyExtensions.Contains(GetExtension(x)));

        // TODO: TBD: re-factored at least for the moment... may want to see about how to make this static in order to better unit test...
        private bool TryRegisterMatchingAssembly(string candidateAssemblyPath, out Assembly assembly)
        {
            assembly = null;
            if (IsNullOrEmpty(candidateAssemblyPath))
            {
                return false;
            }

            //assembly = LoadAssembly(candidateAssemblyPath);
            assembly = PrivateDependencyContext.AddDependency(candidateAssemblyPath)[candidateAssemblyPath];
            RegistrySet.Add(AssemblyDescriptor.Create(candidateAssemblyPath));
            return assembly != null;
        }

        internal Assembly LoadAssembly(AssemblyName assemblyName)
        {
            // ReSharper disable once ImplicitlyCapturedClosure
            Assembly LoadAssemblyByName() => Assembly.Load(assemblyName);

            var loaded = TryRegisterMatchingAssembly(
                GetMatchingReferenceAssemblyPath(AdditionalReferencePaths, assemblyName)
                ?? GetSearchMatchingAssemblyPath(SearchPaths, assemblyName)
                , out var y)
                ? y
                : LoadAssemblyByName();

            return loaded;
        }

        private CompilationAssemblyResolverDependencyContext PrivateDependencyContext { get; }
    }
}
