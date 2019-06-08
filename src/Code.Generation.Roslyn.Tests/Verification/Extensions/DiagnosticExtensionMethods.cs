using System.Collections.Generic;
using System.Linq;

namespace Kingdom.CodeAnalysis.Verifiers
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Text;

    /// <summary>
    /// Provides a set of ubiquitous <see cref="Diagnostic"/> based extension methods.
    /// </summary>
    public static class DiagnosticExtensionMethods
    {
        /// <summary>
        /// Returns the <paramref name="diagnostics"/> Ordered By the <see cref="TextSpan.Start"/>
        /// found in the <see cref="Diagnostic.Location"/> <see cref="Location.SourceSpan"/>.
        /// </summary>
        /// <param name="diagnostics"></param>
        /// <returns></returns>
        public static IEnumerable<Diagnostic> SortDiagnostics(this IEnumerable<Diagnostic> diagnostics)
            => diagnostics.OrderBy(d => d.Location.SourceSpan.Start);
    }
}
