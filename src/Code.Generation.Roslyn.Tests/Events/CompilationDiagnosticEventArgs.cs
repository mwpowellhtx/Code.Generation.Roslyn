using System;
using System.Collections.Generic;

namespace Code.Generation.Roslyn
{
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Signals when Compilation Diagnostics may be evaluated.
    /// </summary>
    /// <inheritdoc />
    public sealed class CompilationDiagnosticEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the Project.
        /// </summary>
        public Project Project { get; }

        /// <summary>
        /// Gets the DiagnosticFilter.
        /// </summary>
        private ICompilationDiagnosticFilter DiagnosticFilter { get; }

        /// <summary>
        /// Gets the Compilation. We must weakly type the <see cref="object"/> this way because
        /// we cannot know the precise type of <see cref="Compilation"/>, especially when things
        /// such as Analyzers are involved.
        /// </summary>
        public object Compilation => DiagnosticFilter.Compilation;

        /// <summary>
        /// Gets the strongly typed <typeparamref name="T"/> <see cref="Compilation"/>.
        /// This is the tradeoff we must make when we want to use the correct strongly typed
        /// <typeparamref name="T"/> Compilation.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetCompilation<T>() where T : class => (T) Compilation;

        /// <summary>
        /// Gets the Diagnostics associated with the <see cref="DiagnosticFilter"/>.
        /// </summary>
        public IEnumerable<Diagnostic> Diagnostics => DiagnosticFilter;

        /// <summary>
        /// Protected Constructor.
        /// </summary>
        /// <param name="project"></param>
        /// <param name="diagnosticFilter"></param>
        /// <inheritdoc />
        internal CompilationDiagnosticEventArgs(Project project, ICompilationDiagnosticFilter diagnosticFilter)
        {
            Project = project;
            DiagnosticFilter = diagnosticFilter;
        }
    }
}
