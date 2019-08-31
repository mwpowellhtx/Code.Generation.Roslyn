using System.Collections.Generic;
using System.Linq;

namespace Code.Generation.Roslyn
{
    using Microsoft.Extensions.DependencyModel.Resolution;

    internal static class ResolverExtensionMethods
    {
        private static IEnumerable<T> GetRange<T>(params T[] values)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var x in values)
            {
                yield return x;
            }
        }

        // TODO: TBD: AFAIK, we need to furnish the Default Resolvers always...
        private static IEnumerable<ICompilationAssemblyResolver> DefaultResolvers
        {
            get
            {
                // TODO: TBD: we require System.Runtime.Loader.AssemblyDependencyResolver somewhere in the mix...
                // https://github.com/dotnet/coreclr/blob/master/src/System.Private.CoreLib/src/System/Runtime/Loader/AssemblyDependencyResolver.cs
                // TODO: TBD: the chief questions are: what is hostpolicy.dll? corehost_resolve_component_dependencies? corehost_set_error_writer?
                // TODO: TBD: are these part of sdk3/netcore3/netstandard3?
                // TODO: TBD: in other words, are they available to sdk2/netcore2/netstandard2?
                // TODO: TBD: and/or how much rolling our own would be required to `back-port´ the issue?
                // TODO: TBD: in and of itself, parsing the JSON itself does not seem like that big of a task...
                // TODO: TBD: we think potentially resolving as a separately deployed project/task might be the best possible approach...
                // TODO: TBD: and just expect/assume that the bits will have all been resolved correctly...
                yield return new ReferenceAssemblyPathResolver();
                yield return new PackageCompilationAssemblyResolver();
            }
        }

        public static CompositeCompilationAssemblyResolver AbsorbResolvers<TResolver>(this TResolver resolver
            , params ICompilationAssemblyResolver[] resolvers)
            where TResolver : ICompilationAssemblyResolver
            => new CompositeCompilationAssemblyResolver(GetRange(
                (ICompilationAssemblyResolver) resolver ?? new CompositeCompilationAssemblyResolver(
                    DefaultResolvers.ToArray())).Concat(resolvers).ToArray());
    }
}
