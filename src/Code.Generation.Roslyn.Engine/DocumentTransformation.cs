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

            Assumes.True(inputDocument.HasCompilationUnitRoot, "Input document expected to include a Compilation Unit.");

            var crLf = CarriageReturnLineFeed;

            IEnumerable<CompilationUnitSyntax> GetTransformations()
            {
                // TODO: TBD: at this level I think it is because we have identified the Document in which the annotation did occur...
                // TODO: TBD: in other words, so any Code Generation attribution has already occurred and been resolved...
                var inputSemanticModel = compilation.GetSemanticModel(inputDocument);
                var inputCompilationUnit = inputDocument.GetCompilationUnitRoot();

                // TODO: TBD: supporting C# today...
                // TODO: TBD: possible for other types of SyntaxNode in the future?
                foreach (var documentNode in new[] {inputDocument.GetRoot() as CSharpSyntaxNode})
                {
                    // TODO: TBD: possibly this gets refactored outside the `foreach´ loop...
                    var attributeData = compilation.GetAttributeData(inputSemanticModel, documentNode);
                    var generators = attributeData.LoadCodeGenerators(ReferenceServiceManager.LoadAssembly).ToArray();

                    foreach (var generator in generators)
                    {
                        var context = new TransformationContext(documentNode, inputSemanticModel, compilation, projectDirectory, inputCompilationUnit);

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
