// Copyright (c) Michael W. Powell. All rights reserved.
// Licensed under the GPLv3 license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Code.Generation.Roslyn
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    /// The only thing that this <see cref="ICodeGenerator"/> does is basically to echo
    /// the <see cref="CompilationUnitSyntax"/> which was discovered as the host for the
    /// <see cref="ReflexiveCodeGenerationByTypeAttribute"/> or
    /// <see cref="ReflexiveCodeGenerationByNameAttribute"/> attributes. We do not expect
    /// that this would necessarily compile successfully in an actual compilation scenario.
    /// The purpose that this serves is to exercise whether the
    /// <see cref="TransformationContext"/> works properly.
    /// </summary>
    /// <inheritdoc />
    public class ReflexiveCodeGenerator : CodeGeneratorBase
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="attributeData"></param>
        /// <inheritdoc />
        public ReflexiveCodeGenerator(AttributeData attributeData) : base(attributeData) { }

        /// <summary>
        /// Now, this is a fairly trivial, if a bit naive Code Generation implementation.
        /// We are solely interested in making sure that fundamental building blocks of
        /// Code Generation, such as inserting a Leading Preamble Text, are taking place
        /// successfully.
        /// </summary>
        /// <inheritdoc />
        public override Task GenerateAsync(TransformationContext context, IProgress<Diagnostic> progress, CancellationToken cancellationToken) => Task.Run(() =>
        {
            Descriptors.Add(new CodeGeneratorDescriptor {CompilationUnits = {context.SourceCompilationUnit}});
        }, cancellationToken);
    }
}
