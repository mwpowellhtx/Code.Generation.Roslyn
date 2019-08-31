using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace Code.Generation.Roslyn
{
    using Microsoft.Extensions.DependencyModel;
    using Microsoft.Extensions.DependencyModel.Resolution;
    using Validation;
    using static StringComparison;
    using static AssemblyLoadContext;
    using static Path;
    using static String;
    using StringBinaryPredicate = BinaryPredicate<string>;

    // TODO: TBD: I think we have a viable refactoring / swap-in replacement for the ARSM internal bits ...
    // TODO: TBD: especially in such a way that we can better verify the individual bits in unit test...
    internal class CompilationAssemblyResolverDependencyContext
    {
        /// <summary>
        /// Gets a new Default instance of a Context.
        /// </summary>
        internal static CompilationAssemblyResolverDependencyContext DefaultContext
            => new CompilationAssemblyResolverDependencyContext();

        /// <summary>
        /// Gets the DefaultComparer. The Default DefaultComparer is the
        /// <see cref="StringComparer.OrdinalIgnoreCase"/>.
        /// </summary>
        /// <see cref="StringComparer.OrdinalIgnoreCase"/>
        internal static StringComparer DefaultComparer { get; set; } = StringComparer.OrdinalIgnoreCase;

        /// <summary>
        /// Gets any Additional Reference Paths. These are intended to augment the default
        /// Dependency Context Assembly Resolution strategy. If an Assembly Reference cannot
        /// be resolved via the Dependency Context and Resolver, these paths serve to augment
        /// that resolution strategy with predetermined references.
        /// </summary>
        internal IReadOnlyCollection<string> AdditionalReferencePaths { get; }

        /// <summary>
        /// Gets the LoadContext.
        /// </summary>
        private AssemblyLoadContext LoadContext { get; }

        /// <summary>
        /// Default Internal Constructor.
        /// </summary>
        /// <inheritdoc />
        internal CompilationAssemblyResolverDependencyContext()
            : this(null)
        {
        }

        /// <summary>
        /// Default Internal Constructor.
        /// </summary>
        /// <param name="additionalReferencePaths"></param>
        /// <see cref="AdditionalReferencePaths"/>
        /// <inheritdoc />
        internal CompilationAssemblyResolverDependencyContext(IReadOnlyCollection<string> additionalReferencePaths)
            : this(additionalReferencePaths, null)
        {
        }

        /// <summary>
        /// Internal Constructor.
        /// </summary>
        /// <param name="additionalReferencePaths"></param>
        /// <param name="comparer"></param>
        /// <see cref="AdditionalReferencePaths"/>
        internal CompilationAssemblyResolverDependencyContext(IReadOnlyCollection<string> additionalReferencePaths, StringComparer comparer)
        {
            AdditionalReferencePaths = additionalReferencePaths ?? Array.Empty<string>();

            comparer = comparer ?? DefaultComparer;

            ResolvedDirectoryPaths = new HashSet<string>(comparer);

            // TODO: TBD: doesn't this get recycled by the GC then?
            LoadContext = GetLoadContext(GetType().GetTypeInfo().Assembly);
            LoadContext.Resolving += OnResolvingLoadContextAssemblyReference;
        }

        private static bool DoesNameEqual(string a, string b, StringComparison comparisonType) => string.Equals(a, b, comparisonType);

        private static bool DoesNameEqual(string a, string b) => DoesNameEqual(a, b, OrdinalIgnoreCase);

        /// <summary>
        /// Event handler occurring On <see cref="AssemblyLoadContext.Resolving"/>.
        /// </summary>
        /// <param name="loadContext"></param>
        /// <param name="assemblyName"></param>
        /// <returns></returns>
        private Assembly OnResolvingLoadContextAssemblyReference(AssemblyLoadContext loadContext, AssemblyName assemblyName)
            => OnResolvingLoadContextAssemblyReference(loadContext, assemblyName, DoesNameEqual);

        private Assembly OnResolvingLoadContextAssemblyReference(AssemblyLoadContext loadContext, AssemblyName assemblyName
            , StringBinaryPredicate predicate)
        {
            bool TryResolveAssemblyPaths(out IEnumerable<string> assemblyPaths)
            {
                // ReSharper disable once RedundantEmptyObjectOrCollectionInitializer
                var paths = (List<string>)(assemblyPaths = new List<string> { });

                var rl = FindMatchingLibrary(Context.RuntimeLibraries, assemblyName, predicate);
                if (rl == null)
                {
                    return false;
                }

                var groups = rl.RuntimeAssemblyGroups;

                var cl = new CompilationLibrary(rl.Type, rl.Name, rl.Version, rl.Hash
                    , groups.SelectMany(g => g.AssetPaths), rl.Dependencies, rl.Serviceable);

                return Resolver.TryResolveAssemblyPaths(cl, paths) && assemblyPaths.Any();
            }

            if (TryResolveAssemblyPaths(out var resolvedPaths))
            {
                return resolvedPaths.Select(loadContext.LoadFromAssemblyPath).FirstOrDefault();
            }

            // TODO: TBD: which, it would seem, `additional´ reference paths are intended to augment the naturally resolved DC.
            return AdditionalReferencePaths.Select(GetFileNameWithoutExtension)
                .Where(f => predicate(f, assemblyName.Name))
                .Select(loadContext.LoadFromAssemblyPath).FirstOrDefault();
        }

        private static TLibrary FindMatchingLibrary<TLibrary>(IEnumerable<TLibrary> libraries, AssemblyName name
            , StringBinaryPredicate predicate)
            where TLibrary : Library
        {
            foreach (var l in libraries)
            {
                if (predicate(l.Name, name.Name))
                {
                    return l;
                }

                /* If the Package Name does not exactly match the Predicate,
                 * then we verify whether the Assembly File Name is a match. */

                if (l is RuntimeLibrary rl
                    && rl.RuntimeAssemblyGroups.Any(
                        g => g.AssetPaths.Select(GetFileNameWithoutExtension)
                            .Any(x => predicate(x, name.Name))
                    )
                )
                {
                    return l;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets or Sets the Resolver.
        /// </summary>
        internal CompositeCompilationAssemblyResolver Resolver { get; private set; }

        //// TODO: TBD: we can use this one if we need to...
        //// TODO: TBD: or does leveraging the Context collections themselves make better sense?
        //private HashSet<string> PathHashSet { get; } = new HashSet<string>(DefaultComparer);

        /// <summary>
        /// Gets the ResolvedDirectoryPaths.
        /// </summary>
        internal HashSet<string> ResolvedDirectoryPaths { get; }

        private IDictionary<string, Assembly> LoadedAssemblies { get; } = new Dictionary<string, Assembly>();

        // TODO: TBD: could make an event(s) out of this that we can listen to internally, for testing purposes, etc...
        // TODO: TBD: additionally, we may not need/want/care about Assembly itself in this regard after all...
        // TODO: TBD: in other words, Assembly is just here because it was part of the initial DC resolution...
        // ReSharper disable once UnusedParameter.Local
        /// <summary>
        /// Occurs when a Dependency was Added to the <see cref="Context"/>.
        /// </summary>
        /// <param name="_">We do not actually use the <see cref="DependencyContext"/> here.</param>
        /// <param name="path"></param>
        /// <param name="assembly"></param>
        /// <see cref="Context"/>
        private void OnDependencyAdded(DependencyContext _, string path, Assembly assembly)
        {
            // TODO: TBD: we receive Context here because it would also be interesting to learn about the DC after merge, resolved, etc.
            void ResolveDirectory(string directoryName)
            {
                if (ResolvedDirectoryPaths.Contains(directoryName))
                {
                    return;
                }

                /* Doing this would turn around and inform Resolver.TryResolveAssemblyPaths(...)
                 * when it comes time to resolve Assemblies. */

                Resolver = Resolver.AbsorbResolvers(new AppBaseCompilationAssemblyResolver(directoryName));
                ResolvedDirectoryPaths.Add(directoryName);
            }

            ResolveDirectory(GetDirectoryName(path));

            // TODO: TBD: check the keys... should only add paths whose filename, sans directory path, should be added.
            LoadedAssemblies.Add(path, assembly);
        }

        /// <summary>
        /// Gets the Context. The Default Context is <see cref="DependencyContext.Default"/>.
        /// </summary>
        /// <see cref="DependencyContext"/>
        /// <see cref="DependencyContext.Default"/>
        internal DependencyContext Context { get; private set; } = DependencyContext.Default;

        /// <summary>
        /// Adds the Dependency given the Assembly File <paramref name="path"/>.
        /// Loads a Load Context given the component class type host Assembly.
        /// </summary>
        /// <param name="path"></param>
        /// <returns>Returns the Dependency Context instance.</returns>
        internal CompilationAssemblyResolverDependencyContext AddDependency(string path)
        {
            // TODO: TBD: in which case, we are looking for an actual, honest to goodness, Assembly file name path...
            var assembly = LoadContext.LoadFromAssemblyPath(path);
            return AddDependency(path, assembly);
        }

        // TODO: TBD: could potentially refactor this one as an extension method...
        /// <summary>
        /// Returns whether the <paramref name="library"/> Exists given
        /// <paramref name="expectedPath"/> and <paramref name="comparisonType"/> criteria.
        /// </summary>
        /// <typeparam name="TLibrary"></typeparam>
        /// <param name="library"></param>
        /// <param name="expectedPath"></param>
        /// <param name="comparisonType"></param>
        /// <returns></returns>
        internal static bool HasLibrary<TLibrary>(TLibrary library, string expectedPath
            , StringComparison comparisonType = OrdinalIgnoreCase)
            where TLibrary : Library
        {
            var fileName = GetFileNameWithoutExtension(expectedPath);

            return library.Path?.Equals(expectedPath, comparisonType) == true
                   || library.Name.Equals(fileName, comparisonType);
        }

        /// <summary>
        /// Adds the Dependency <paramref name="assembly"/> to the <see cref="Context"/>
        /// provided it has not already been Added. Also Initializes <see cref="Context"/>
        /// in the event it has not yet seen an <paramref name="assembly"/> Dependency.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="assembly"></param>
        /// <returns>Returns the Dependency Context instance.</returns>
        /// <exception cref="CodeGenerationDependencyException">
        /// Thrown when <paramref name="path"/> <see cref="DependencyContext"/> cannot be loaded.
        /// </exception>
        internal CompilationAssemblyResolverDependencyContext AddDependency(string path, Assembly assembly)
        {
            Requires.NotNull(assembly, nameof(assembly));

            bool DoesHaveLibrary<TLibrary>(TLibrary x)
                where TLibrary : Library
                => HasLibrary(x, path);

            // ReSharper disable once InvertIf
            if (!(Context.CompileLibraries.Any(DoesHaveLibrary) || Context.RuntimeLibraries.Any(DoesHaveLibrary)))
            {
                try
                {
                    // TODO: TBD: falling over, apparently, for reasons related to:
                    // TODO: TBD: Improve task DLL load behavior / https://github.com/microsoft/msbuild/issues/1312
                    // TODO: TBD: the current suspicion is that `identifying´ references is insufficient...
                    // TODO: TBD: we may need to actually attempt to load those `additional´ references.
                    // TODO: TBD: what we do not want to end up doing, if we can avoid it, is loading a swath of Dot Net, or System, assemblies, if we can at all help it...
                    // ReSharper disable once IdentifierTypo
                    var assyDependencyContext = DependencyContext.Load(assembly);
                    Requires.NotNull(assyDependencyContext, nameof(assyDependencyContext));
                    Context = Context.Merge(assyDependencyContext);
                }
                catch (Exception ex)
                {
                    throw new CodeGenerationDependencyException($"Unable to load dependency `{path}´.", path, ex);
                }

                OnDependencyAdded(Context, path, assembly);
            }

            return this;
        }

        /// <summary>
        /// Gets the <see cref="Assembly"/> Indexed corresponding to the Loaded
        /// <paramref name="path"/>. Assume that <see cref="AddDependency(string)"/> was invoked
        /// prior to Indexing the Path. Returns the corresponding <see cref="Assembly"/> Reference
        /// from <see cref="AdditionalReferencePaths"/> when loaded context fails.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Assembly this[string path] => this[path, OrdinalIgnoreCase];

        /// <summary>
        /// Gets the <see cref="Assembly"/> Indexed corresponding to the Loaded
        /// <paramref name="path"/> according to the <paramref name="comparisonType"/>. Assume
        /// that <see cref="AddDependency(string)"/> was invoked prior to Indexing the Path.
        /// Returns the corresponding <see cref="Assembly"/> Reference from
        /// <see cref="AdditionalReferencePaths"/> when loaded context fails.
        /// </summary>
        /// <param name="path">We are interested in identifying the Resolved or Augmented Path.</param>
        /// <param name="comparisonType">Overriding the default <see cref="StringComparison"/> is allowed.</param>
        /// <returns></returns>
        public Assembly this[string path, StringComparison comparisonType]
        {
            get
            {
                // TODO: TBD: we could potentially instruct a search heuristic to search based on Equals, or possible also Containing...
                Assembly GetAdditionalReference()
                {
                    // TODO: TBD: should these be `augmenting´ the normally added dependencies?
                    // TODO: TBD: or should these be fed into the normally resolved dependency context?

                    // TODO: TBD: may capture a default comparison type applicable over the range of questions... i.e. OrdinalIgnoreCase
                    var chosen = AdditionalReferencePaths.SingleOrDefault(
                        x => !IsNullOrEmpty(x)
                             && GetFileName(x).Equals(GetFileName(path), comparisonType)
                    );

                    return !IsNullOrEmpty(chosen) && File.Exists(chosen) ? Assembly.LoadFile(GetFullPath(chosen)) : null;
                }

                return LoadedAssemblies.ContainsKey(path) ? LoadedAssemblies[path] : GetAdditionalReference();
            }
        }
    }
}
