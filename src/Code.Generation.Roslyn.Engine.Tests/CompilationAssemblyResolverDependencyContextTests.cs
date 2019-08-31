using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Code.Generation.Roslyn
{
    using Microsoft.Extensions.DependencyModel.Resolution;
    using Xunit;
    using Xunit.Abstractions;
    using static BindingFlags;
    using static StringComparison;
    using static CompilationAssemblyResolverDependencyContext;
    using static Path;

    public class CompilationAssemblyResolverDependencyContextTests : TestFixtureBase
    {
        /// <summary>
        /// Gets a new instance of <see cref="CompilationAssemblyResolverDependencyContext"/>.
        /// </summary>
        private static CompilationAssemblyResolverDependencyContext DefaultContext
            => CompilationAssemblyResolverDependencyContext.DefaultContext
                .AssertNotNull()
                .AssertTrue(x => !x.AdditionalReferencePaths.AssertNotNull().Any());

        public CompilationAssemblyResolverDependencyContextTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        [Fact]
        public void Has_Valid_Subject_Instance()
        {
            // ReSharper disable once UnusedParameter.Local
            // Just here as a placeholder so that the Parameter is allowed to evaluate.
            void Verify(object _) => _.AssertNotNull();

            Verify(DefaultContext);
        }

        [Fact]
        public void Default_Context_Default_Resolved_Directory_Paths_Correct()
        {
            void Verify(CompilationAssemblyResolverDependencyContext dc)
                => dc.ResolvedDirectoryPaths.AssertNotNull().AssertEmpty();

            Verify(DefaultContext);
        }

        /// <summary>
        /// &quot;bin&quot;
        /// </summary>
        private const string Bin = "bin";

        /// <summary>
        /// &quot;..\..\..\..\&quot;
        /// </summary>
        private const string RelativePathRoot = @"..\..\..\..\";

        /// <summary>
        /// &quot;Debug&quot;
        /// </summary>
        private const string Configuration = "Debug";

        /// <summary>
        /// &quot;netstandard2.0&quot;
        /// </summary>
        private const string TargetFramework = "netstandard2.0";

        /// <summary>
        /// &quot;Kingdom.Simple.Calculators&quot;
        /// </summary>
        private const string KingdomSimpleCalculators = "Kingdom.Simple.Calculators";

        /// <summary>
        /// &quot;_resolvers&quot;
        /// </summary>
        private const string InternalResolvers = "_resolvers";

        /// <summary>
        /// <see cref="NonPublic"/> | <see cref="Instance"/>
        /// </summary>
        /// <see cref="NonPublic"/>
        /// <see cref="Instance"/>
        private const BindingFlags InternalResolversBindingFlags = NonPublic | Instance;

        // ReSharper disable once StringLiteralTypo
        /// <summary>
        /// Gets the Calculators Assembly Path.
        /// </summary>
        private static string KingdomSimpleCalculatorsPath
            => $@"{RelativePathRoot}{KingdomSimpleCalculators}\{Bin}\{Configuration}\{TargetFramework}\{KingdomSimpleCalculators}.dll";

        [Fact]
        public void Resolvers_Correct_After_Standalone_Reference_Added()
        {
            void Verify(string path, ref CompilationAssemblyResolverDependencyContext dc
                , Action<CompositeCompilationAssemblyResolver, ICompilationAssemblyResolver[]> verify)
            {
                path = GetFullPath(path.AssertFileExists());

                dc = dc ?? DefaultContext.AssertTrue(x => x.AssertNotNull().Context.AssertNotNull() != null);

                var ac = dc;

                ac.AssertSame(dc);

                ac.AddDependency(path.AssertFileExists());

                var actualResolver = ac.Resolver.AssertNotNull();

                // TODO: TBD: a deeper question here is this: should ReferenceAssemblyPathResolver/PackageCompilationAssemblyResolver happen with every added dependency? or just the first one?
                // TODO: TBD: might be worth reviewing the original code and determine how that was done... and/or contact the author(s) to determine the finer points...
                // With deeper levels simply walking lambda arguments to the next letters.
                actualResolver.AssertReflectedField(InternalResolvers, InternalResolversBindingFlags, verify);
            }

            CompilationAssemblyResolverDependencyContext actualContext = null;

            void Verification(CompositeCompilationAssemblyResolver v, ICompilationAssemblyResolver[] f)
            {
                f.AssertNotNull().AssertNotEmpty().AssertTrue(x => x.Length == 2);

                f[0].AssertNotNull().AssertIsType<CompositeCompilationAssemblyResolver>()
                    .AssertReflectedField(InternalResolvers, InternalResolversBindingFlags
                        , (CompositeCompilationAssemblyResolver w, ICompilationAssemblyResolver[] g) =>
                        {
                            g.AssertNotNull().AssertNotEmpty().AssertTrue(x => x.Length == 2);
                            g[0].AssertNotNull().AssertIsType<ReferenceAssemblyPathResolver>();
                            g[1].AssertNotNull().AssertIsType<PackageCompilationAssemblyResolver>();
                        });

                f[1].AssertNotNull().AssertIsType<AppBaseCompilationAssemblyResolver>();
            }

            var firstPath = KingdomSimpleCalculatorsPath.AssertNotNull().AssertNotEmpty();

            // TODO: TBD: consider refactoring a general use private static method for use with both standalone and chaining dependencies...
            Verify(firstPath, ref actualContext, Verification);

            var expectedContextContext = actualContext.Context.AssertNotNull();

            actualContext.AssertNotNull();

            Verify(firstPath.AssertNotNull().AssertNotEmpty(), ref actualContext, Verification);

            actualContext.AssertNotNull().Context.AssertNotNull().AssertSame(expectedContextContext);

            expectedContextContext.AssertTrue(
                x => x.CompileLibraries.Any(y => HasLibrary(y, firstPath))
                     || x.RuntimeLibraries.Any(y => HasLibrary(y, firstPath))
            );
        }

        /// <summary>
        /// &quot;Kingdom.Simple.Services&quot;
        /// </summary>
        private const string KingdomSimpleServices = "Kingdom.Simple.Services";

        /// <summary>
        /// &quot;KingdomSimpleServices}.dll&quot;
        /// </summary>
        private static string KingdomSimpleServicesFileName => $"{KingdomSimpleServices}.dll";

        // ReSharper disable once StringLiteralTypo
        /// <summary>
        /// Gets the Services Assembly Path.
        /// </summary>
        private static string KingdomSimpleServicesPath
            => $@"{RelativePathRoot}{KingdomSimpleServices}\{Bin}\{Configuration}\{TargetFramework}\{KingdomSimpleServicesFileName}";

        /// <summary>
        /// &quot;Kingdom.Simple.Services.Definitions&quot;
        /// </summary>
        private const string KingdomSimpleServicesDefinitions = "Kingdom.Simple.Services.Definitions";

        // ReSharper disable once StringLiteralTypo
        /// <summary>
        /// Gets the Services Assembly Path.
        /// </summary>
        private static string KingdomSimpleServicesDefinitionsPath
            => $@"{RelativePathRoot}{KingdomSimpleServicesDefinitions}\{Bin}\{Configuration}\{TargetFramework}\{KingdomSimpleServicesDefinitions}.dll";

        [Fact]
        public void Resolvers_Correct_After_Dependency_References_Added()
        {
            void Verify(string path, ref CompilationAssemblyResolverDependencyContext dc
                , Action<CompositeCompilationAssemblyResolver, ICompilationAssemblyResolver[]> verify)
            {
                path = GetFullPath(path.AssertFileExists());

                dc = dc ?? DefaultContext.AssertTrue(x => x.AssertNotNull().Context.AssertNotNull() != null);

                var ac = dc;

                ac.AssertSame(dc);

                ac.AddDependency(path.AssertFileExists());

                var actualResolver = ac.Resolver.AssertNotNull();

                actualResolver.AssertReflectedField(InternalResolvers, InternalResolversBindingFlags, verify);
            }

            // TODO: TBD: first of all... this is correct? As we fold in more references, the resolution folds in on itself?
            CompilationAssemblyResolverDependencyContext actualContext = null;

            void BaseVerification(CompositeCompilationAssemblyResolver v, ICompilationAssemblyResolver[] f)
            {
                f.AssertNotNull().AssertNotEmpty().AssertTrue(x => x.Length == 2);

                f[0].AssertNotNull().AssertIsType<CompositeCompilationAssemblyResolver>()
                    .AssertReflectedField(InternalResolvers, InternalResolversBindingFlags
                        , (CompositeCompilationAssemblyResolver w, ICompilationAssemblyResolver[] g) =>
                        {
                            g.AssertNotNull().AssertNotEmpty().AssertTrue(x => x.Length == 2);
                            g[0].AssertNotNull().AssertIsType<ReferenceAssemblyPathResolver>();
                            g[1].AssertNotNull().AssertIsType<PackageCompilationAssemblyResolver>();
                        });

                f[1].AssertNotNull().AssertIsType<AppBaseCompilationAssemblyResolver>();
            }

            var firstPath = KingdomSimpleServicesDefinitionsPath.AssertNotNull().AssertNotEmpty();

            // With deeper levels simply walking lambda arguments to the next letters.
            Verify(firstPath, ref actualContext, BaseVerification);

            var expectedContextContext = actualContext.Context.AssertNotNull();

            expectedContextContext.AssertTrue(
                x => x.CompileLibraries.Any(y => HasLibrary(y, firstPath))
                     || x.RuntimeLibraries.Any(y => HasLibrary(y, firstPath))
            );

            var secondPath = KingdomSimpleServicesPath.AssertNotNull().AssertNotEmpty();

            Verify(secondPath, ref actualContext
                , (v, f) =>
                {
                    f.AssertNotNull().AssertNotEmpty();
                    f.AssertTrue(x => x.Length == 2);

                    f[0].AssertNotNull().AssertIsType<CompositeCompilationAssemblyResolver>()
                        .AssertReflectedField(InternalResolvers, InternalResolversBindingFlags
                            , (CompositeCompilationAssemblyResolver w, ICompilationAssemblyResolver[] g) =>
                            {
                                g.AssertNotNull().AssertNotEmpty().AssertTrue(x => x.Length == 2);

                                g[0].AssertIsType<CompositeCompilationAssemblyResolver>()
                                    .AssertReflectedField(InternalResolvers, InternalResolversBindingFlags
                                        , (CompositeCompilationAssemblyResolver x, ICompilationAssemblyResolver[] h) =>
                                        {
                                            h.AssertNotNull().AssertNotEmpty().AssertTrue(y => y.Length == 2);
                                            h[0].AssertNotNull().AssertIsType<ReferenceAssemblyPathResolver>();
                                            h[1].AssertNotNull().AssertIsType<PackageCompilationAssemblyResolver>();
                                        });

                                g[1].AssertNotNull().AssertIsType<AppBaseCompilationAssemblyResolver>();
                            });

                    f[1].AssertNotNull().AssertIsType<AppBaseCompilationAssemblyResolver>();
                });

            var actualContextContext = actualContext.AssertNotNull().Context.AssertNotNull();

            actualContextContext.AssertNotSame(expectedContextContext);

            actualContextContext.AssertTrue(
                x => x.CompileLibraries.Any(y => HasLibrary(y, secondPath))
                     || x.RuntimeLibraries.Any(y => HasLibrary(y, secondPath))
            );
        }

        /// <summary>
        /// Gets a new instance of <see cref="CompilationAssemblyResolverDependencyContext"/>.
        /// </summary>
        private static CompilationAssemblyResolverDependencyContext ContextWithAdditionalReferences
            => new CompilationAssemblyResolverDependencyContext(GetRange(KingdomSimpleServicesPath).ToArray())
                .AssertNotNull()
                .AssertEqual(1, x => x.AdditionalReferencePaths.AssertNotNull().Count);

        private static void VerifyContextWithAdditionalReferences(CompilationAssemblyResolverDependencyContext dc
            , string expectedPath, Action<Assembly> verification)
        {
            var attemptedAssemblyResolution = dc.AssertNotNull()[expectedPath.AssertNotNull().AssertNotEmpty()];
            verification.AssertNotNull().Invoke(attemptedAssemblyResolution);
        }

        private static void VerifyContextWithAdditionalReferences(CompilationAssemblyResolverDependencyContext dc
            , string expectedPath, StringComparison comparisonType, Action<Assembly> verification)
        {
            var attemptedAssemblyResolution = dc.AssertNotNull()[expectedPath.AssertNotNull().AssertNotEmpty(), comparisonType];
            verification.AssertNotNull().Invoke(attemptedAssemblyResolution);
        }

        [Fact]
        public void Context_Does_Resolve_Assuming_Additional_References()
            => VerifyContextWithAdditionalReferences(ContextWithAdditionalReferences
                , KingdomSimpleServicesFileName, assembly => assembly.AssertNotNull());

        [Fact]
        public void Context_Does_Not_Resolve_Failing_Assumed_Additional_References()
            => VerifyContextWithAdditionalReferences(DefaultContext
                , KingdomSimpleServicesFileName, assembly => assembly.AssertNull());

        [Fact]
        public void Context_Does_Resolve_Assuming_Additional_References_With_Comparison_Index()
            => VerifyContextWithAdditionalReferences(ContextWithAdditionalReferences, KingdomSimpleServicesFileName
                , OrdinalIgnoreCase, assembly => assembly.AssertNotNull());

        [Fact]
        public void Context_Does_Not_Resolve_Failing_Assumed_Additional_References_With_Comparison_Index()
            => VerifyContextWithAdditionalReferences(DefaultContext, KingdomSimpleServicesFileName
                , OrdinalIgnoreCase, assembly => assembly.AssertNull());
    }
}
