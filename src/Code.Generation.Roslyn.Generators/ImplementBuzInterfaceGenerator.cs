// Copyright (c) Michael W. Powell. All rights reserved.
// Licensed under the GPLv3 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Code.Generation.Roslyn.Generators
{
    using Microsoft.CodeAnalysis;
    using static SyntaxFactory;
    using static Integration.ModuleKind;
    using static SyntaxKind;

    /// <summary>
    /// This Generator implements the fairly innocuous <see cref="IDisposable"/> pattern for
    /// the decorated class. This is a fairly non-trivial, fairly common, want to do from time
    /// to time. There is no reason why a <see cref="ICodeGenerator"/> could not do this mundane
    /// work for the subscriber.
    /// </summary>
    /// <inheritdoc />
    public class ImplementBuzInterfaceGenerator : CodeGeneratorBase
    {
        /// <summary>
        /// Public Constructor.
        /// </summary>
        /// <param name="attributeData"></param>
        /// <inheritdoc />
        public ImplementBuzInterfaceGenerator(AttributeData attributeData) : base(attributeData) { }

        private static AccessorDeclarationSyntax GetAccessorDecl => AccessorDeclaration(GetAccessorDeclaration);

        private static AccessorDeclarationSyntax SetAccessorDecl => AccessorDeclaration(SetAccessorDeclaration);

        private static SyntaxToken EndOfStatementToken => Token(SemicolonToken);

        private static SyntaxToken BooleanKeywordToken => Token(BoolKeyword);

        private static SyntaxToken PartialKeywordToken => Token(PartialKeyword);

        private static SyntaxToken PrivateKeywordToken => Token(PrivateKeyword);

        private static SyntaxToken ProtectedKeywordToken => Token(ProtectedKeyword);

        private static SyntaxToken PublicKeywordToken => Token(PublicKeyword);

        private static SyntaxToken VirtualKeywordToken => Token(VirtualKeyword);

        private static SyntaxToken VoidKeywordToken => Token(VoidKeyword);

        private static ExpressionSyntax TrueLiteral => LiteralExpression(TrueLiteralExpression);

        /// <summary>
        /// Returns the <see cref="UsingDirectiveSyntax"/> for the <typeparamref name="T"/>
        /// <see cref="Type.Namespace"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <see cref="Type.Namespace"/>
        private static UsingDirectiveSyntax GetUsingNamespaceDirective<T>()
            => UsingDirective(IdentifierName(typeof(T).Namespace));

        private static MemberDeclarationSyntax GetDisposeMethodDecl(string methodName, string disposingName)
            => MethodDeclaration(PredefinedType(VoidKeywordToken), Identifier(methodName))
                .WithModifiers(TokenList(ProtectedKeywordToken, VirtualKeywordToken))
                .WithParameterList(ParameterList(
                    SingletonSeparatedList(
                        Parameter(Identifier(disposingName)).WithType(PredefinedType(BooleanKeywordToken)))
                ))
                .WithBody(Block());

        private static MemberDeclarationSyntax GetIsDisposedPropertyDecl(string propertyName)
            => PropertyDeclaration(PredefinedType(BooleanKeywordToken), Identifier(propertyName))
                .WithModifiers(TokenList(ProtectedKeywordToken))
                .WithAccessorList(AccessorList(List(
                    new[]
                    {
                        GetAccessorDecl.WithSemicolonToken(EndOfStatementToken),
                        SetAccessorDecl.WithModifiers(TokenList(PrivateKeywordToken))
                            .WithSemicolonToken(EndOfStatementToken)
                    })));

        private static MemberDeclarationSyntax GetDisposeImplementationMethodDecl(string methodName, string disposedPropertyName)
            => MethodDeclaration(PredefinedType(VoidKeywordToken), Identifier(methodName))
                .WithModifiers(TokenList(PublicKeywordToken))
                .WithBody(Block(IfStatement(
                        PrefixUnaryExpression(LogicalNotExpression, IdentifierName(disposedPropertyName)),
                        Block(SingletonList<StatementSyntax>(
                            ExpressionStatement(InvocationExpression(IdentifierName(methodName))
                                .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(TrueLiteral))))
                            )
                        ))),
                    ExpressionStatement(AssignmentExpression(
                        SimpleAssignmentExpression
                        , IdentifierName(disposedPropertyName)
                        , TrueLiteral
                    ))
                ));

        /// <summary>
        /// Now, this is a fairly trivial, if a bit naive Code Generation implementation.
        /// We are solely interested in making sure that fundamental building blocks of
        /// Code Generation, such as inserting a Leading Preamble Text, are taking place
        /// successfully.
        /// </summary>
        /// <inheritdoc />
        public override Task GenerateAsync(TransformationContext context, IProgress<Diagnostic> progress, CancellationToken cancellationToken)
        {
            const string dispose = nameof(IDisposable.Dispose);
            const string disposing = nameof(disposing);
            // ReSharper disable once InconsistentNaming
            const string IsDisposed = nameof(IsDisposed);

            IEnumerable<CodeGeneratorDescriptor> Generate()
            {
                var namespaceDecl = context.SourceCompilationUnit.ChildNodes().OfType<NamespaceDeclarationSyntax>().Single();
                var classDecl = context.SourceCompilationUnit.ChildNodes().OfType<ClassDeclarationSyntax>().Single();

                yield return new CodeGeneratorDescriptor
                {
                    CompilationUnits =
                    {
                        CompilationUnit()
                            .WithUsings(SingletonList(GetUsingNamespaceDirective<IDisposable>()))
                            .WithMembers(SingletonList<MemberDeclarationSyntax>(
                                NamespaceDeclaration(namespaceDecl.Name)
                                    .WithMembers(SingletonList<MemberDeclarationSyntax>(
                                        ClassDeclaration(classDecl.Identifier)
                                            .WithModifiers(TokenList(PublicKeywordToken, PartialKeywordToken))
                                            .WithBaseList(BaseList(
                                                SingletonSeparatedList<BaseTypeSyntax>(
                                                    SimpleBaseType(IdentifierName($"I{Buz}"))
                                                )
                                            ))
                                            .WithMembers(List(new[]
                                            {
                                                GetDisposeMethodDecl(dispose, disposing),
                                                GetIsDisposedPropertyDecl(IsDisposed),
                                                GetDisposeImplementationMethodDecl(dispose, IsDisposed)
                                            }))
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
