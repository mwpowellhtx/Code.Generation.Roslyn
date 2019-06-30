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

    /// <summary>
    /// Whereas <see cref="DocumentTransformation"/> services <see cref="SyntaxTree"/> oriented
    /// Transformations, this <see cref="ServiceManager"/> operates at the Assembly level during
    /// Compilation.
    /// </summary>
    /// <inheritdoc />
    public class AssemblyTransformation : TransformationBase<AssemblyTransformationContext, AssemblyTransformation>
    {
        /// <summary>
        /// Internal Constructor.
        /// </summary>
        /// <param name="referenceService"></param>
        /// <inheritdoc />
        internal AssemblyTransformation(AssemblyReferenceServiceManager referenceService)
            : base(referenceService)
        {
        }

        /// <inheritdoc />
        public override Task<IEnumerable<CompilationUnitSyntax>> TransformAsync(CSharpCompilation compilation
            , IProgress<Diagnostic> progress, CancellationToken cancellationToken)
        {
            Requires.NotNull(compilation, nameof(compilation));

            var crLf = CarriageReturnLineFeed;

            IEnumerable<CompilationUnitSyntax> GetTransformations()
            {
                var attributeData = compilation.GetAssemblyAttributeData();
                var generators = attributeData.LoadCodeGenerators<IAssemblyCodeGenerator>(ReferenceService.LoadAssembly).ToArray();

                foreach (var generator in generators)
                {
                    var context = new AssemblyTransformationContext(compilation, ProjectDirectory);

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

            return Task.Run(GetTransformations, cancellationToken);
        }
    }
}
