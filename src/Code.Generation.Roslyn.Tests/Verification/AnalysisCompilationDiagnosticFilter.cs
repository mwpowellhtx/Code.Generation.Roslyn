using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace Code.Generation.Roslyn
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;

    /// <summary>
    /// Represents a <see cref="CompilationWithAnalyzers"/> centric <see cref="Diagnostic"/>
    /// filter.
    /// </summary>
    /// <inheritdoc />
    public class AnalysisCompilationDiagnosticFilter : CompilationDiagnosticFilter
    {
        /// <summary>
        /// Public Constructor.
        /// </summary>
        /// <param name="compilation"></param>
        /// <inheritdoc />
        public AnalysisCompilationDiagnosticFilter(CompilationWithAnalyzers compilation)
            : base(compilation, default)
        {
        }

        /// <summary>
        /// Public Constructor.
        /// </summary>
        /// <param name="compilation"></param>
        /// <param name="cancellationToken"></param>
        /// <inheritdoc />
        public AnalysisCompilationDiagnosticFilter(CompilationWithAnalyzers compilation, CancellationToken cancellationToken)
            : base(compilation, cancellationToken)
        {
        }

        /// <summary>
        /// The default behavior simply yields
        /// <see cref="CompilationWithAnalyzers.GetAllDiagnosticsAsync(CancellationToken)"/>
        /// instances. Override if you want to perform a different kind
        /// of <see cref="Diagnostic"/> filtration.
        /// </summary>
        /// <inheritdoc />
        protected override Task<ImmutableArray<Diagnostic>> DiagnosticsAsync
            => GetCompilation<CompilationWithAnalyzers>().GetAllDiagnosticsAsync(CancellationToken);
    }
}
