using System.Linq;

namespace Code.Generation.Roslyn
{
    using Microsoft.Extensions.DependencyModel.Resolution;

    internal static class ResolverExtensionMethods
    {
        public static CompositeCompilationAssemblyResolver AbsorbResolvers<TResolver>(this TResolver resolver, params ICompilationAssemblyResolver[] resolvers)
            where TResolver : ICompilationAssemblyResolver
            => new CompositeCompilationAssemblyResolver(resolvers.Concat(new ICompilationAssemblyResolver[] {resolver}).ToArray());
    }
}
