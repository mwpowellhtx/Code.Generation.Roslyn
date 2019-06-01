// TODO: TBD: ditto licensing...
// Copyright (c) 2019 Michael W. Powell. All rights reserved.
// Licensed under the MS-PL license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Code.Generation.Roslyn
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    /// At its root Code Generation relay an enumeration of <see cref="CodeGeneratorDescriptor"/>.
    /// </summary>
    /// <inheritdoc />
    public interface ICodeGenerator : IEnumerable<CodeGeneratorDescriptor>
    {
        /// <summary>
        /// Initiates the enumeration of your <see cref="CompilationUnitSyntax"/>
        /// with discoverable <paramref name="context"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="progress"></param>
        /// <param name="cancellationToken"></param>
        /// <see cref="TransformationContext"/>
        /// <returns></returns>
        Task GenerateAsync(TransformationContext context, IProgress<Diagnostic> progress, CancellationToken cancellationToken);
    }
}
