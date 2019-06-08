using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Code.Generation.Roslyn
{
    // TODO: TBD: we are pretty confident this focuses solely on the Diagnostic issue...
    // TODO: TBD: how ever we arrived at the compilation itself, when we want diagnostics to occur, we can filter them...
    /// <summary>
    /// Compilation <see cref="Diagnostic"/> Filter screens the Compilation Diagnostic results.
    /// Unfortunately, we cannot identify type as a true <see cref="Compilation"/>, on account
    /// of API such as <see cref="DiagnosticAnalyzerExtensions"/> yields an entirely different
    /// kind of class. Literally, <see cref="CompilationWithAnalyzers"/> is a wrapper only, not
    /// a derived class. However, both yield a set of <see cref="Diagnostic"/> instances, via
    /// different API. So the best we can do at this level is to Adapt a Filter around that
    /// concern.
    /// </summary>
    /// <inheritdoc />
    public abstract class CompilationDiagnosticFilter : ICompilationDiagnosticFilter
    {
        /// <inheritdoc />
        public object Compilation { get; }

        /// <inheritdoc />
        public T GetCompilation<T>() where T : class => (T) Compilation;

        /// <summary>
        /// Gets the associated CancellationToken.
        /// </summary>
        protected CancellationToken CancellationToken { get; }

        /// <summary>
        /// Override in order to yield the set of <see cref="Diagnostic"/> instances.
        /// We do this as a <see cref="Task{TResult}"/> because this is how some of the
        /// <see cref="Compilation"/> API work.
        /// </summary>
        protected abstract Task<ImmutableArray<Diagnostic>> DiagnosticsAsync { get; }

        /// <summary>
        /// Gets the Default <see cref="Predicate"/>.
        /// </summary>
        public static DiagnosticFilterPredicate DefaultPredicate => _ => true;

        /// <summary>
        /// Protected Constructor.
        /// </summary>
        /// <param name="compilation"></param>
        /// <param name="cancellationToken"></param>
        protected CompilationDiagnosticFilter(object compilation, CancellationToken cancellationToken)
        {
            Compilation = compilation;
            CancellationToken = cancellationToken;
            Predicate = DefaultPredicate;
        }

        /// <summary>
        /// Gets or Sets the Predicate. Default yields <value>true</value>, we allow All
        /// <see cref="Diagnostic"/> instances to pass through. In some instances, you may want
        /// to furnish your own Predicate, to filter, let us consider, errors only, for instance.
        /// </summary>
        /// <see cref="DiagnosticFilterPredicate"/>
        /// <inheritdoc />
        public DiagnosticFilterPredicate Predicate { get; set; }

        // TODO: TBD: may want to capture them apart from such a dynamic set of diagnostics...
        /// <inheritdoc />
        public virtual IEnumerator<Diagnostic> GetEnumerator() => DiagnosticsAsync.Result.Where(Predicate.Invoke).GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
