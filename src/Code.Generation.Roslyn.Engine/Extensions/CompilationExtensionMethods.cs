using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Code.Generation.Roslyn
{
    using Microsoft.CodeAnalysis;
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

            IEnumerable<AttributeData> MineForAttributeData() => syntaxNode.DescendantNodesAndSelf()
                .SelectMany(node => document.GetDeclaredSymbol(node)?.GetAttributes()
                                    ?? ImmutableArray<AttributeData>.Empty);

            return MineForAttributeData().ToImmutableArray();
        }
    }
}
