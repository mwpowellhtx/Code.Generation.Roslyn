using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Code.Generation.Roslyn
{
    using Microsoft.CodeAnalysis;
    using Validation;

    internal static class CompilationExtensionMethods
    {
        public static ImmutableArray<AttributeData> GetAssemblyAttributeData<TCompilation>(this TCompilation compilation)
            where TCompilation : Compilation
        {
            Requires.NotNull(compilation, nameof(compilation));
            Requires.NotNull(compilation.Assembly, nameof(compilation.Assembly));

            IEnumerable<AttributeData> MineForAttributeData() => compilation.Assembly.GetAttributes();

            return MineForAttributeData().ToImmutableArray();
        }

        public static ImmutableArray<AttributeData> GetAttributeData<TCompilation>(
            this TCompilation compilation, SemanticModel document, SyntaxNode syntaxNode)
            where TCompilation : Compilation
        {
            Requires.NotNull(compilation, nameof(compilation));
            Requires.NotNull(document, nameof(document));
            Requires.NotNull(syntaxNode, nameof(syntaxNode));

            IEnumerable<AttributeData> MineForAttributeData()
            {
                foreach (var y in syntaxNode.DescendantNodesAndSelf().SelectMany(
                    x => document.GetDeclaredSymbol(x)?.GetAttributes()
                         ?? ImmutableArray<AttributeData>.Empty))
                {
                    yield return y;
                }
            }

            return MineForAttributeData().ToImmutableArray();
        }
    }
}
