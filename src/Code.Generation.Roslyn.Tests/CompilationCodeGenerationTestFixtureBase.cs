using System.Linq;

namespace Code.Generation.Roslyn
{
    using Microsoft.CodeAnalysis.CSharp;
    using Xunit.Abstractions;

    public abstract class CompilationCodeGenerationTestFixtureBase<TCompilationManager> : TestFixtureBase
        where TCompilationManager : CompilationManager, new()
    {
        protected TCompilationManager CompilationManager { get; }

        protected virtual void CompilationManager_OnEvaluateCompilation(object sender, CompilationDiagnosticEventArgs e)
        {
        }

        // TODO: TBD: may do this on a more case-by-case, Fact-by-Fact, or Theory, basis...
        // TODO: TBD: or even establish a handful of different fit for purpose test fixtures...
        protected virtual void CompilationManager_OnResolveMetadataReferences(object sender, ResolveMetadataReferencesEventArgs e)
        {
            // TODO: TBD: refactored these references from the compilation manager as a next step...
            // TODO: TBD: I expect that we may refactor these even further from here, but this seems like the logical next step...
            var references = GetRange(
                "mscorlib.dll"
                , "netstandard.dll"
                , "System.dll"
                , "System.Core.dll"
                , "System.Runtime.dll"
            ).ToArray();

            // TODO: TBD: if NETCOREAPP? "System.Private.CoreLib.dll"
            e.AddTypeAssemblyLocationBasedReferences<object>(references)
                .AddReferenceToTypeAssembly<CSharpCompilation>()
                .AddReferenceToTypeAssembly<CodeGenerationAttributeAttribute>()
                .AddReferenceToTypeAssembly<TestAttributeBase>()
                ;
        }

        protected CompilationCodeGenerationTestFixtureBase(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            CompilationManager = new TCompilationManager();

            CompilationManager.ResolveMetadataReferences += CompilationManager_OnResolveMetadataReferences;
            CompilationManager.EvaluateCompilation += CompilationManager_OnEvaluateCompilation;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !IsDisposed)
            {
                if (CompilationManager != null)
                {
                    CompilationManager.ResolveMetadataReferences -= CompilationManager_OnResolveMetadataReferences;
                    CompilationManager.EvaluateCompilation -= CompilationManager_OnEvaluateCompilation;
                }

                CompilationManager?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
