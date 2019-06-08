using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace Code.Generation.Roslyn
{
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Represents a <see cref="Compilation"/> centric <see cref="Diagnostic"/> filter.
    /// </summary>
    /// <inheritdoc />
    public class BasicCompilationDiagnosticFilter : CompilationDiagnosticFilter
    {
        /// <summary>
        /// Public Constructor.
        /// </summary>
        /// <param name="compilation"></param>
        /// <inheritdoc />
        public BasicCompilationDiagnosticFilter(Compilation compilation)
            : base(compilation, default)
        {
        }

        /// <summary>
        /// Public Constructor.
        /// </summary>
        /// <param name="compilation"></param>
        /// <inheritdoc />
        public BasicCompilationDiagnosticFilter(Compilation compilation, CancellationToken cancellationToken)
            : base(compilation, cancellationToken)
        {
        }

        /// <summary>
        /// Default behavior returns <see cref="Compilation.GetDiagnostics"/>. Override if you
        /// want to do a more specialized sourcing of <see cref="Diagnostic"/> instances, such
        /// as <see cref="Compilation.GetParseDiagnostics"/>, for instance.
        /// </summary>
        /// <inheritdoc />
        protected override Task<ImmutableArray<Diagnostic>> DiagnosticsAsync
            => Task.Run(() => GetCompilation<Compilation>().GetDiagnostics(), CancellationToken);
    }
}
