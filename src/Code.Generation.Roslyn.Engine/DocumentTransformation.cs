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
    using Validation;
    using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

    public class DocumentTransformation : ServiceManager
    {
        /// <summary>
        /// Gets the ReferenceService.
        /// </summary>
        private AssemblyReferenceServiceManager ReferenceServiceManager { get; }

        internal DocumentTransformation(AssemblyReferenceServiceManager referenceServiceManager)
        {
            Verify.Operation(referenceServiceManager != null, FormatVerifyOperationMessage(nameof(referenceServiceManager)));

            ReferenceServiceManager = referenceServiceManager;
        }

        // TODO: TBD: return set of CompilationUnitSyntax ...
        /// <summary>
        /// Transforms the current <paramref name="compilation"/> in terms of a set
        /// of <see cref="CompilationUnitSyntax"/>.
        /// </summary>
        /// <param name="compilation"></param>
        /// <param name="inputDocument"></param>
        /// <param name="projectDirectory"></param>
        /// <param name="progress"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <see cref="ReferenceServiceManager"/>
        public Task<IEnumerable<CompilationUnitSyntax>> TransformAsync(CSharpCompilation compilation
            , SyntaxTree inputDocument, string projectDirectory, IProgress<Diagnostic> progress
            , CancellationToken cancellationToken
        )
        {
            Requires.NotNull(compilation, nameof(compilation));
            Requires.NotNull(inputDocument, nameof(inputDocument));
            Requires.NotNull(ReferenceServiceManager, nameof(ReferenceServiceManager));

            IEnumerable<CompilationUnitSyntax> GetTransformations()
            {
                // TODO: TBD: at this level I think it is because we have identified the Document in which the annotation did occur...
                // TODO: TBD: in other words, so any Code Generation attribution has already occurred and been resolved...
                var inputSemanticModel = compilation.GetSemanticModel(inputDocument);
                var inputCompilationUnit = inputDocument.GetCompilationUnitRoot();

                // TODO: TBD: will have to keep an eye on this part... especially in terms of what all can be decorated for code gen.
                // TODO: TBD: i.e. Class, Struct, Interface, Enum, Module (CompilationUnitSyntax?), Assembly (?) ...
                // Because Namespace and Type Declarations are both a kind of Member Declaration.
                var documentNodes = inputDocument
                    .GetRoot()
                    .DescendantNodesAndSelf(n => n is CompilationUnitSyntax || n is MemberDeclarationSyntax) // ?
                    .OfType<CSharpSyntaxNode>();

                foreach (var documentNode in documentNodes)
                {
                    var attributeData = compilation.GetAttributeData(inputSemanticModel, documentNode);
                    var generators = attributeData.FindCodeGenerators(ReferenceServiceManager.LoadAssembly);

                    foreach (var generator in generators)
                    {
                        var context = new TransformationContext(documentNode, inputSemanticModel, compilation
                            , projectDirectory, inputCompilationUnit);

                        generator.GenerateAsync(context, progress, cancellationToken).Wait(cancellationToken);

                        foreach (var descriptor in generator)
                        {
                            foreach (var compilationUnit in descriptor.CompilationUnits)
                            {
                                // Should preserve any `whitespace' that occurs thereafter.
                                var innerCompilationUnit = compilationUnit.NormalizeWhitespace();

                                innerCompilationUnit = innerCompilationUnit.WithLeadingTrivia(
                                    Comment(descriptor.PreambleCommentText)
                                );

                                if (descriptor.IncludeEndingNewLine)
                                {
                                    innerCompilationUnit = innerCompilationUnit.WithTrailingTrivia(
                                        CarriageReturnLineFeed
                                    );
                                }

                                yield return innerCompilationUnit;
                            }
                        }
                    }
                }
            }

            return Task.Run(GetTransformations, cancellationToken);
        }
    }
}
