// Copyright (c) Michael W. Powell. All rights reserved.
// Licensed under the GPLv3 license. See LICENSE file in the project root for full license information.

namespace Code.Generation.Roslyn
{
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Derive from this class in order to perform <see cref="DocumentTransformationContext"/>
    /// oriented Code Generation.
    /// </summary>
    /// <inheritdoc cref="IDocumentCodeGenerator"/>
    /// <see cref="DocumentTransformationContext"/>
    public abstract class DocumentCodeGenerator : CodeGenerator<DocumentTransformationContext>, IDocumentCodeGenerator
    {
        /// <summary>
        /// Protected Constructor.
        /// </summary>
        /// <param name="attributeData"></param>
        /// <inheritdoc />
        protected DocumentCodeGenerator(AttributeData attributeData)
            : base(attributeData)
        {
        }
    }
}
