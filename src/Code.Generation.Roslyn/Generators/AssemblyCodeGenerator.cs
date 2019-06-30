// Copyright (c) Michael W. Powell. All rights reserved.
// Licensed under the GPLv3 license. See LICENSE file in the project root for full license information.

namespace Code.Generation.Roslyn
{
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Derive from this class in order to perform <see cref="AssemblyTransformationContext"/>
    /// oriented Code Generation.
    /// </summary>
    /// <inheritdoc cref="IAssemblyCodeGenerator"/>
    /// <see cref="AssemblyTransformationContext"/>
    public abstract class AssemblyCodeGenerator : CodeGenerator<AssemblyTransformationContext>, IAssemblyCodeGenerator
    {
        /// <summary>
        /// Protected Constructor.
        /// </summary>
        /// <param name="attributeData"></param>
        /// <inheritdoc />
        protected AssemblyCodeGenerator(AttributeData attributeData)
            : base(attributeData)
        {
        }
    }
}
