// Copyright (c) Michael W. Powell. All rights reserved.
// Licensed under the GPLv3 license. See LICENSE file in the project root for full license information.

namespace Code.Generation.Roslyn
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    // TODO: TBD: could potentially be informed by a Context base class...
    /// <summary>
    /// Provides all the inputs and context necessary to perform the code generation.
    /// </summary>
    public class DocumentTransformationContext : TransformationContextBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentTransformationContext" /> class.
        /// </summary>
        /// <param name="compilation">The compilation for which code is being Generated.</param>
        /// <param name="processingNode">The node at which point the generator attribute is
        /// discovered.</param>
        /// <param name="semanticModel">The semantic model for the
        /// <paramref name="compilation"/>.</param>
        /// <param name="projectDirectory">The absolute path of the Project Directory
        /// corresponding to the <paramref name="compilation"/>.</param>
        /// <param name="sourceCompilationUnit">The Source <see cref="CompilationUnitSyntax"/>
        /// at which point Code Generation was triggered.</param>
        /// <inheritdoc />
        internal DocumentTransformationContext(CSharpCompilation compilation, CSharpSyntaxNode processingNode
            , SemanticModel semanticModel, string projectDirectory, CompilationUnitSyntax sourceCompilationUnit)
            : base(compilation)
        {
            ProcessingNode = processingNode;
            SemanticModel = semanticModel;
            ProjectDirectory = projectDirectory;
            SourceCompilationUnit = sourceCompilationUnit;
        }

        /// <summary>
        /// Gets the node at which point the generator attribute is discovered.
        /// </summary>
        /// <see cref="CodeGenerationAttributeAttribute">The Attribute full name
        /// as directed by this attribute.</see>
        public CSharpSyntaxNode ProcessingNode { get; }

        /// <summary>
        /// Gets the semantic model for the <see cref="Compilation" />.
        /// </summary>
        public SemanticModel SemanticModel { get; }

        /// <summary>
        /// Gets the absolute path of the the project directory corresponding
        /// to the <see cref="Compilation"/>.
        /// </summary>
        public string ProjectDirectory { get; }

        /// <summary>
        /// The Source <see cref="CompilationUnitSyntax"/> at which point Code Generation
        /// was triggered.
        /// </summary>
        public CompilationUnitSyntax SourceCompilationUnit { get; }
    }
}
