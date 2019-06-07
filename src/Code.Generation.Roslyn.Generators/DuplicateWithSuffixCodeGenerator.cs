// Copyright (c) Michael W. Powell. All rights reserved.
// Licensed under the GPLv3 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Code.Generation.Roslyn
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    public class DuplicateWithSuffixCodeGenerator : CodeGeneratorBase
    {
        private string Suffix { get; }

        public DuplicateWithSuffixCodeGenerator(AttributeData attributeData)
            : base(attributeData)
        {
            Suffix = (string) attributeData.ConstructorArguments[0].Value;
        }

        // TODO: TBD: if we do refactor, sans this element... or provide as a Generators assembly extension method based upon SyntaxNode ?
        /// <summary>
        /// Finds the most recent <see cref="NamespaceDeclarationSyntax"/>.
        /// Such Declarations may be Nested. It is up to the Caller to discern
        /// what this means to the Code Generator.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected static IEnumerable<NamespaceDeclarationSyntax> FindNamespaceDeclarations(SyntaxNode node)
        {
            IEnumerable<T> SelectWhile<T>(T x, Func<T, bool> predicate, Func<T, T> transform)
            {
                bool mayContinue;
                do
                {
                    yield return x;
                    mayContinue = predicate(x);
                    x = transform(x);
                } while (mayContinue);
            }

            return SelectWhile(node, x => x.Parent != null, x => x.Parent).Reverse().OfType<NamespaceDeclarationSyntax>();
        }

        public override Task GenerateAsync(TransformationContext context, IProgress<Diagnostic> progress, CancellationToken cancellationToken)
        {
            // TODO: TBD: must inform the Descriptors ...
            var results = SyntaxFactory.List<MemberDeclarationSyntax>();

            var namespaceDecls = FindNamespaceDeclarations(context.ProcessingNode);

            MemberDeclarationSyntax copy = null;
            var applyToClass = context.ProcessingNode as ClassDeclarationSyntax;
            if (applyToClass != null)
            {
                copy = applyToClass.WithIdentifier(
                    SyntaxFactory.Identifier($"{applyToClass.Identifier.ValueText}{Suffix}")
                );
            }

            if (copy != null)
            {
                results = results.Add(copy);
            }

            return Task.FromResult(results);
        }
    }
}
