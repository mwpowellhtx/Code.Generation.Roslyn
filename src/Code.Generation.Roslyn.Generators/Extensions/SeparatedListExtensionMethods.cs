using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;

namespace Code.Generation.Roslyn
{
    using Microsoft.CodeAnalysis;
    using static SyntaxFactory;
    using static SyntaxKind;

    internal static class SeparatedListExtensionMethods
    {
        public static IEnumerable<SyntaxNodeOrToken> Separate<TNode>(this IEnumerable<TNode> nodes, SyntaxKind delimiter = CommaToken)
            where TNode : SyntaxNode
        {
            // ReSharper disable PossibleMultipleEnumeration
            for (var i = 0; i < nodes.Count(); ++i)
            {
                if (i > 0)
                {
                    yield return Token(delimiter);
                }

                yield return nodes.ElementAt(i);
            }
            // ReSharper restore PossibleMultipleEnumeration
        }
    }
}
