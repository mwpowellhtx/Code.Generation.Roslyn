// TODO: TBD: ditto licensing...
// Copyright (c) 2019 Michael W. Powell. All rights reserved.
// Licensed under the MS-PL license. See LICENSE.txt file in the project root for full license information.

namespace Code.Generation.Roslyn
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    /// Provides all the inputs and context necessary to perform the code generation.
    /// </summary>
    public class TransformationContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TransformationContext" /> class.
        /// </summary>
        /// <param name="processingNode">The node at which point the generator attribute is
        /// discovered.</param>
        /// <param name="semanticModel">The semantic model for the
        /// <paramref name="compilation"/>.</param>
        /// <param name="compilation">The compilation for which code is being Generated.</param>
        /// <param name="projectDirectory">The absolute path of the Project Directory
        /// corresponding to the <paramref name="compilation"/>.</param>
        /// <param name="sourceCompilationUnit">The Source <see cref="CompilationUnitSyntax"/>
        /// at which point Code Generation was triggered.</param>
        public TransformationContext(
            CSharpSyntaxNode processingNode
            , SemanticModel semanticModel
            , CSharpCompilation compilation
            , string projectDirectory
            , CompilationUnitSyntax sourceCompilationUnit
        )
        {
            ProcessingNode = processingNode;
            SemanticModel = semanticModel;
            Compilation = compilation;
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
        /// Gets the <see cref="CSharpCompilation"/> for which the code being generated.
        /// </summary>
        public CSharpCompilation Compilation { get; }

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
