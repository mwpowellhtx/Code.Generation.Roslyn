// Copyright (c) Michael W. Powell. All rights reserved.
// Licensed under the GPLv3 license. See LICENSE file in the project root for full license information.

namespace Code.Generation.Roslyn
{
    using Microsoft.CodeAnalysis.CSharp;

    // TODO: TBD: could potentially be informed by a Context base class...
    /// <summary>
    /// Provides all the inputs and context necessary to perform the code generation.
    /// </summary>
    public class AssemblyTransformationContext : TransformationContextBase
    {
        // ReSharper disable once CommentTypo
        // ReSharper disable once InheritdocConsiderUsage
        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyTransformationContext" /> class.
        /// </summary>
        /// <param name="compilation">The compilation for which code is being Generated.</param>
        /// <param name="projectDirectory">The absolute path of the Project Directory
        /// corresponding to the <paramref name="compilation"/>.</param>
        internal AssemblyTransformationContext(CSharpCompilation compilation, string projectDirectory)
            : base(compilation)
        {
            ProjectDirectory = projectDirectory;
        }

        /// <summary>
        /// Gets the absolute path of the the project directory corresponding to the
        /// <see cref="TransformationContextBase.Compilation"/>.
        /// </summary>
        public string ProjectDirectory { get; }
    }
}
