// Copyright (c) Michael W. Powell. All rights reserved.
// Licensed under the GPLv3 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace Code.Generation.Roslyn
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Validation;

    public class DuplicateWithSuffixCodeGenerator : CodeGeneratorBase
    {
        private AttributeData AttributeData { get; }

        private ImmutableDictionary<string, TypedConstant> Data { get; }

        private string Suffix { get; }

        public DuplicateWithSuffixCodeGenerator(AttributeData attributeData)
        {
            Requires.NotNull(attributeData, nameof(attributeData));

            AttributeData = attributeData;

            Suffix = (string) attributeData.ConstructorArguments[0].Value;

            Data = AttributeData.NamedArguments.ToImmutableDictionary(pair => pair.Key, pair => pair.Value);
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
