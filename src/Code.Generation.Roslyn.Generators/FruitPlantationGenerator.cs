// Copyright (c) Michael W. Powell. All rights reserved.
// Licensed under the GPLv3 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;

namespace Code.Generation.Roslyn.Generators
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using static SyntaxFactory;
    using static SyntaxKind;

    /// <summary>
    /// This Generator implements the fairly innocuous <see cref="IDisposable"/> pattern for
    /// the decorated assembly. This is a fairly non-trivial, fairly common, want to do from time
    /// to time. There is no reason why a <see cref="IAssemblyCodeGenerator"/> could not do this
    /// mundane work for the subscriber.
    /// </summary>
    /// <inheritdoc />
    public class FruitPlantationGenerator : AssemblyCodeGenerator
    {
        /// <summary>
        /// Public Constructor.
        /// </summary>
        /// <param name="attributeData"></param>
        /// <inheritdoc />
        public FruitPlantationGenerator(AttributeData attributeData) : base(attributeData) { }

        /// <summary>
        /// The only reason we allow this to be Public is on account of the Integration Tests
        /// approach, which requires accessibility to be consistent at a compilation level. This
        /// has absolutely nothing to do with the validity of the code generation itself, however.
        /// Otherwise, we could allow virtually any valid accessibility, Internal, for instance.
        /// That would be no problem, whatsoever.
        /// </summary>
        private static SyntaxToken PublicToken => Token(PublicKeyword);

        private static NamespaceDeclarationSyntax GetNameSpace(string identifier)
            => NamespaceDeclaration(IdentifierName(identifier));

        private static EnumDeclarationSyntax GetEnumeration(string enumIdentifier, params string[] enumMemberIdentifiers)
            => EnumDeclaration(enumIdentifier)
                .WithModifiers(SyntaxTokenList.Create(PublicToken))
                .WithMembers(SeparatedList<EnumMemberDeclarationSyntax>(
                    enumMemberIdentifiers.Select(y => EnumMemberDeclaration(Identifier(y))).Separate()
                ));

        /// <summary>
        /// Now, this is a fairly trivial, if a bit naive Code Generation implementation.
        /// We are solely interested in making sure that fundamental building blocks of
        /// Code Generation, such as inserting a Leading Preamble Text, are taking place
        /// successfully.
        /// </summary>
        /// <inheritdoc />
        public override Task GenerateAsync(AssemblyTransformationContext context, IProgress<Diagnostic> progress
            , CancellationToken cancellationToken)
        {
            // ReSharper disable InconsistentNaming
            const string Foo = nameof(Foo);
            const string FruitKind = nameof(FruitKind);
            const string Apple = nameof(Apple);
            const string Orange = nameof(Orange);
            const string Banana = nameof(Banana);
            const string Lime = nameof(Lime);
            const string Lemon = nameof(Lemon);
            const string Cherry = nameof(Cherry);
            // ReSharper restore InconsistentNaming

            IEnumerable<CodeGeneratorDescriptor> Generate()
            {
                yield return new CodeGeneratorDescriptor
                {
                    CompilationUnits =
                    {
                        CompilationUnit()
                            .WithMembers(SingletonList<MemberDeclarationSyntax>(
                                GetNameSpace(Foo)
                                    .WithMembers(SingletonList<MemberDeclarationSyntax>(
                                        GetEnumeration(FruitKind, Apple, Orange, Banana, Lime, Lemon, Cherry)
                                    ))
                            ))
                    }
                };
            }

            void RunGenerate()
            {
                foreach (var d in Generate())
                {
                    Descriptors.Add(d);
                }
            }

            return Task.Run(RunGenerate, cancellationToken);
        }
    }
}
