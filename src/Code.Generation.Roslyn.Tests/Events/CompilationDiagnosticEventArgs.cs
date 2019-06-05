using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Code.Generation.Roslyn
{
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Event arguments provided for purposes of Evaluating the <see cref="Compilation"/>
    /// for better or for worse.
    /// </summary>
    /// <inheritdoc />
    public class CompilationDiagnosticEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the Compilation.
        /// </summary>
        public Compilation Compilation { get; }

        /// <summary>
        /// Gets the Diagnostics associated with the Compilation.
        /// </summary>
        public IEnumerable<Diagnostic> Diagnostics { get; }

        /// <summary>
        /// Internal Constructor.
        /// </summary>
        /// <param name="compilation"></param>
        /// <param name="cancellationToken"></param>
        /// <inheritdoc />
        internal CompilationDiagnosticEventArgs(Compilation compilation, CancellationToken cancellationToken = default)
        {
            Compilation = compilation;
            Diagnostics = compilation.GetDiagnostics(cancellationToken).ToArray();
        }
    }
}
