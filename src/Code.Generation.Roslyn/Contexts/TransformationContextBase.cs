// Copyright (c) Michael W. Powell. All rights reserved.
// Licensed under the GPLv3 license. See LICENSE file in the project root for full license information.

namespace Code.Generation.Roslyn
{
    using Microsoft.CodeAnalysis.CSharp;

    public class TransformationContextBase
    {
        /// <summary>
        /// Gets the <see cref="CSharpCompilation"/> for which the code being generated.
        /// </summary>
        public CSharpCompilation Compilation { get; }

        /// <summary>
        /// Protected Constructor.
        /// </summary>
        /// <param name="compilation"></param>
        protected TransformationContextBase(CSharpCompilation compilation)
        {
            Compilation = compilation;
        }
    }
}
