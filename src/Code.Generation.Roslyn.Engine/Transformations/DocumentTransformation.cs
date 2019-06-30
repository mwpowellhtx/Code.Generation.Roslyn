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

    public class DocumentTransformation : TransformationBase<DocumentTransformationContext, DocumentTransformation>
    {
        /// <summary>
        /// Internal Constructor.
        /// </summary>
        /// <param name="referenceService"></param>
        /// <inheritdoc />
        internal DocumentTransformation(AssemblyReferenceServiceManager referenceService)
            : base(referenceService) { }

        private SyntaxTree _inputDocument;

        /// <summary>
        /// Gets or Sets the InputDocument.
        /// </summary>
        internal SyntaxTree InputDocument
        {
            get => _inputDocument;
            set
            {
                Requires.NotNull(value, nameof(value));
                Assumes.True(value.HasCompilationUnitRoot, $"{nameof(InputDocument)} expected to include a Compilation Unit.");
                _inputDocument = value;
            }
        }

        /// <inheritdoc />
        public override Task<IEnumerable<CompilationUnitSyntax>> TransformAsync(CSharpCompilation compilation
            , IProgress<Diagnostic> progress, CancellationToken cancellationToken
        )
        {
            Requires.NotNull(compilation, nameof(compilation));
            Requires.NotNull(InputDocument, nameof(InputDocument));

            var crLf = CarriageReturnLineFeed;

            // TODO: TBD: to a point, some of this code looks very similar, could be refactored to base class...
            IEnumerable<CompilationUnitSyntax> GetTransformations()
            {
                // TODO: TBD: at this level I think it is because we have identified the Document in which the annotation did occur...
                // TODO: TBD: in other words, so any Code Generation attribution has already occurred and been resolved...
                var inputSemanticModel = compilation.GetSemanticModel(InputDocument);
                var inputCompilationUnit = InputDocument.GetCompilationUnitRoot();

                // TODO: TBD: supporting C# today...
                // TODO: TBD: possible for other types of SyntaxNode in the future?
                foreach (var documentNode in new[] {InputDocument.GetRoot() as CSharpSyntaxNode})
                {
                    // TODO: TBD: possibly this gets refactored outside the `foreach´ loop...
                    var attributeData = compilation.GetAttributeData(inputSemanticModel, documentNode);
                    var generators = attributeData.LoadCodeGenerators<IDocumentCodeGenerator>(ReferenceService.LoadAssembly).ToArray();

                    foreach (var generator in generators)
                    {
                        var context = new DocumentTransformationContext(compilation, documentNode, inputSemanticModel, ProjectDirectory, inputCompilationUnit);

                        generator.GenerateAsync(context, progress, cancellationToken).Wait(cancellationToken);

                        foreach (var descriptor in generator)
                        {
                            foreach (var compilationUnit in descriptor.CompilationUnits)
                            {
                                // Should preserve any `whitespace´ that occurs thereafter.
                                var innerCompilationUnit = compilationUnit.NormalizeWhitespace();

                                // Mind the whitespace, that is critical.
                                if (descriptor.PreambleCommentText.Any())
                                {
                                    innerCompilationUnit = innerCompilationUnit.WithLeadingTrivia(Comment(descriptor.PreambleCommentText.Trim()), crLf);
                                }

                                if (descriptor.IncludeEndingNewLine)
                                {
                                    innerCompilationUnit = innerCompilationUnit.WithTrailingTrivia(crLf);
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
