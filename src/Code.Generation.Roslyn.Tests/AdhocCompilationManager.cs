using System;

namespace Code.Generation.Roslyn
{
    using Microsoft.CodeAnalysis;

    // ReSharper disable once IdentifierTypo
    /// <summary>
    /// The default <see cref="CompilationManager{T}"/> leverages support using an
    /// <see cref="AdhocWorkspace"/> instance.
    /// </summary>
    /// <see cref="!:https://glosbe.com/la/en/Ad-Hoc"/>
    /// <inheritdoc />
    public class AdhocCompilationManager : CompilationManager<AdhocWorkspace>
    {
        /// <summary>
        /// Gets the <see cref="Lazy{T}"/> <see cref="AdhocWorkspace"/> instance.
        /// </summary>
        /// <inheritdoc />
        protected override Lazy<AdhocWorkspace> LazyWorkspace { get; }
            = new Lazy<AdhocWorkspace>(() => new AdhocWorkspace());
    }
}
