using System.Collections.Immutable;
using System.Linq;

namespace Code.Generation.Roslyn
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Validation;

    internal static class CompilationExtensionMethods
    {
        public static ImmutableArray<AttributeData> GetAttributeData<TCompilation>(
            this TCompilation compilation, SemanticModel document, SyntaxNode syntaxNode)
            where TCompilation : Compilation
        {
            Requires.NotNull(compilation, nameof(compilation));
            Requires.NotNull(document, nameof(document));
            Requires.NotNull(syntaxNode, nameof(syntaxNode));

            switch (syntaxNode)
            {
                case CompilationUnitSyntax syntax:
                    return compilation.Assembly.GetAttributes()
                        .Where(x => x.ApplicationSyntaxReference.SyntaxTree == syntax.SyntaxTree)
                        .ToImmutableArray();

                default:
                    return document.GetDeclaredSymbol(syntaxNode)?.GetAttributes() ?? ImmutableArray<AttributeData>.Empty;
            }
        }
    }
}
