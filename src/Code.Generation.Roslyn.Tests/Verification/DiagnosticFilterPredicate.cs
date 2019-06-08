namespace Code.Generation.Roslyn
{
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Represents a <paramref name="diagnostic"/> Predicate for the
    /// <see cref="CompilationDiagnosticFilter{TCompilation}"/>.
    /// </summary>
    /// <param name="diagnostic"></param>
    /// <returns></returns>
    public delegate bool DiagnosticFilterPredicate(Diagnostic diagnostic);
}
