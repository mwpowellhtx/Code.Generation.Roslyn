using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;

namespace Code.Generation.Roslyn
{
    using Microsoft.Extensions.DependencyModel;
    using Microsoft.Extensions.DependencyModel.Resolution;

    class Testing
    {
        private DependencyContext dependencyContext;
        private readonly Dictionary<string, Assembly> assembliesByPath = new Dictionary<string, Assembly>();
        private readonly HashSet<string> directoriesWithResolver = new HashSet<string>();
        private CompositeCompilationAssemblyResolver assemblyResolver;

        public IReadOnlyList<string> ReferencePath { get; set; }

        protected Assembly LoadAssembly(string path)
        {
            if (this.assembliesByPath.ContainsKey(path))
                return this.assembliesByPath[path];

            var loadContext = AssemblyLoadContext.GetLoadContext(this.GetType().GetTypeInfo().Assembly);
            var assembly = loadContext.LoadFromAssemblyPath(path);

            var newDependencyContext = DependencyContext.Load(assembly);
            if (newDependencyContext != null)
                this.dependencyContext = this.dependencyContext.Merge(newDependencyContext);
            var basePath = Path.GetDirectoryName(path);
            if (!this.directoriesWithResolver.Contains(basePath))
            {
                this.assemblyResolver = new CompositeCompilationAssemblyResolver(new ICompilationAssemblyResolver[]
                {
                    new AppBaseCompilationAssemblyResolver(basePath),
                    this.assemblyResolver,
                });
                this.directoriesWithResolver.Add(basePath);
            }

            this.assembliesByPath.Add(path, assembly);
            return assembly;
        }

        private Assembly ResolveAssembly(AssemblyLoadContext context, AssemblyName name)
        {
            var library = FindMatchingLibrary(this.dependencyContext.RuntimeLibraries, name);
            if (library == null)
                return null;
            var wrapper = new CompilationLibrary(
                library.Type,
                library.Name,
                library.Version,
                library.Hash,
                library.RuntimeAssemblyGroups.SelectMany(g => g.AssetPaths),
                library.Dependencies,
                library.Serviceable);

            var assemblyPaths = new List<string>();
            this.assemblyResolver.TryResolveAssemblyPaths(wrapper, assemblyPaths);

            if (assemblyPaths.Count == 0)
            {
                var matches = from refAssemblyPath in this.ReferencePath
                              where Path.GetFileNameWithoutExtension(refAssemblyPath).Equals(name.Name, StringComparison.OrdinalIgnoreCase)
                              select context.LoadFromAssemblyPath(refAssemblyPath);
                return matches.FirstOrDefault();
            }

            return assemblyPaths.Select(context.LoadFromAssemblyPath).FirstOrDefault();
        }

        private static RuntimeLibrary FindMatchingLibrary(IEnumerable<RuntimeLibrary> libraries, AssemblyName name)
        {
            foreach (var runtime in libraries)
            {
                if (string.Equals(runtime.Name, name.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return runtime;
                }

                // If the NuGet package name does not exactly match the AssemblyName,
                // we check whether the assembly file name is matching
                if (runtime.RuntimeAssemblyGroups.Any(
                        g => g.AssetPaths.Any(
                            p => string.Equals(Path.GetFileNameWithoutExtension(p), name.Name, StringComparison.OrdinalIgnoreCase))))
                {
                    return runtime;
                }
            }
            return null;
        }
    }

    class MoreTesting : Testing
    {
        public IReadOnlyList<string> GeneratorAssemblySearchPaths { get; set; }

        private static readonly HashSet<string> AllowedAssemblyExtensions
            = new HashSet<string>(StringComparer.OrdinalIgnoreCase) {".dll"};

        private readonly List<string> loadedAssemblies = new List<string>();

        private Assembly LoadAssembly(AssemblyName assemblyName)
        {
            var matchingRefAssemblies = from refPath in this.ReferencePath
                where Path.GetFileNameWithoutExtension(refPath).Equals(assemblyName.Name, StringComparison.OrdinalIgnoreCase)
                select refPath;
            var matchingAssemblies = from path in this.GeneratorAssemblySearchPaths
                from file in Directory.EnumerateFiles(path, $"{assemblyName.Name}.dll", SearchOption.TopDirectoryOnly)
                where AllowedAssemblyExtensions.Contains(Path.GetExtension(file))
                select file;

            string matchingRefAssembly = matchingRefAssemblies.Concat(matchingAssemblies).FirstOrDefault();
            if (matchingRefAssembly != null)
            {
                this.loadedAssemblies.Add(matchingRefAssembly);
                return this.LoadAssembly(matchingRefAssembly);
            }

            return Assembly.Load(assemblyName);
        }
    }
}
