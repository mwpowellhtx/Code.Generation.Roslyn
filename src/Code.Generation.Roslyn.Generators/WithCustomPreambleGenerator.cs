// Copyright (c) Michael W. Powell. All rights reserved.
// Licensed under the GPLv3 license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Code.Generation.Roslyn
{
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Similar to the <see cref="ReflexiveCodeGenerator"/>, excepting for the
    /// <see cref="CustomPreambleText"/>.
    /// </summary>
    /// <inheritdoc />
    public class WithCustomPreambleGenerator : DocumentCodeGenerator
    {
        /// <summary>
        /// Literally, &quot;// Custom Preamble Text&quot;.
        /// </summary>
        /// <remarks>Does not have to be that fancy, just indicate we have something other
        /// than the canned preamble text.</remarks>
        internal const string CustomPreambleText = @"// Custom Preamble Text";

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="attributeData"></param>
        /// <inheritdoc />
        public WithCustomPreambleGenerator(AttributeData attributeData) : base(attributeData) { }

        /// <summary>
        /// Now, this is a fairly trivial, if a bit naive Code Generation implementation.
        /// We are solely interested in making sure that fundamental building blocks of
        /// Code Generation, such as inserting a Leading Preamble Text, are taking place
        /// successfully.
        /// </summary>
        /// <inheritdoc />
        public override Task GenerateAsync(DocumentTransformationContext context, IProgress<Diagnostic> progress
            , CancellationToken cancellationToken)
            => Task.Run(
                () =>
                {
                    Descriptors.Add(new CodeGeneratorDescriptor
                    {
                        CompilationUnits = {context.SourceCompilationUnit}, PreambleCommentText = CustomPreambleText
                    });
                }, cancellationToken);
    }
}
