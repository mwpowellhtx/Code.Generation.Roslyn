using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Code.Generation.Roslyn
{
    using Microsoft.CodeAnalysis.CSharp;
    using Xunit.Abstractions;
    using static Constants;
    using static String;
    using static DiagnosticSeverity;

    /// <summary>
    /// The <see cref="Compilation"/> is the thing for which this Manager is driving. Everything
    /// here prepares for and initiates a Compilation then furnishes the <see cref="Diagnostic"/>
    /// results following said Compilation.
    /// </summary>
    /// <typeparam name="TWorkspace"></typeparam>
    /// <typeparam name="TCompilationManager"></typeparam>
    public abstract class CompilationCodeGenerationTestFixtureBase<TWorkspace, TCompilationManager> : TestFixtureBase
        where TWorkspace : Workspace
        where TCompilationManager : CompilationManager<TWorkspace>, new()
    {
        protected TCompilationManager CompilationManager { get; }

        protected virtual void ReportDiagnostic(Diagnostic diagnostic)
        {
            // TODO: TBD: we need to know more than just Diagnostic in order to connect the dots with the ErrorMessage?
            var result = DiagnosticResult.Create(diagnostic);

            //// TODO: TBD: what sort of 'Location' are we talking about here?
            //result.Locations = new [] {diagnostic.Location}.Concat(diagnostic.AdditionalLocations.ToArray()).ToArray();

            OutputHelper.WriteLine(result.Summary);
        }

        /// <summary>
        /// Override to handle more than the basic <see cref="Compilation.GetDiagnostics"/>
        /// validation. <see cref="Diagnostic"/> validation is the first thing we must rule out.
        /// Because if there is any problem actually compiling, then there is not much point
        /// performing any further validation or analysis.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void CompilationManager_OnEvaluateCompilation(object sender, CompilationDiagnosticEventArgs e)
        {
            // TODO: TBD: which we might capture in some sort of options...
            const DiagnosticSeverity minimumSeverity = Error;

            bool FilterDiagnosticSeverity(Diagnostic diagnostic) => diagnostic.Severity >= minimumSeverity;

            e.Compilation.AssertIsAssignableFrom<Compilation>().AssertSame(e.Compilation);

            var diagnostics = e.Diagnostics.AssertNotNull().Where(FilterDiagnosticSeverity).ToList();

            diagnostics.ForEach(ReportDiagnostic);

            diagnostics.Any().AssertFalse();
        }

        // TODO: TBD: may do this on a more case-by-case, Fact-by-Fact, or Theory, basis...
        // TODO: TBD: or even establish a handful of different fit for purpose test fixtures...
        protected virtual void CompilationManager_OnResolveMetadataReferences(object sender, ResolveMetadataReferencesEventArgs e)
        {
            //var trusted = ((string) AppContext.GetData($"TRUSTED_PLATFORM_ASSEMBLIES")).Split(';');

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
            // TODO: TBD: this needs to be loaded regardless whether this is a NETCOREAPP ...
            e.AddReferenceToTypeAssembly<object>();
            e.AddTypeAssemblyLocationBasedReferences<object>(references);
            e.AddReferenceToTypeAssembly<CSharpCompilation>();
            e.AddReferenceToTypeAssembly<CodeGenerationAttributeAttribute>();
            e.AddReferenceToTypeAssembly<TestAttributeBase>();
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
