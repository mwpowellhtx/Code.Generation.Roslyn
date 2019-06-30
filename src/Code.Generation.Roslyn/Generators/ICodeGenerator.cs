// Copyright (c) Michael W. Powell. All rights reserved.
// Licensed under the GPLv3 license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Code.Generation.Roslyn
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    /// Represents the basic Code Generation concerns.
    /// </summary>
    /// <inheritdoc />
    public interface ICodeGenerator : IEnumerable<CodeGeneratorDescriptor>
    {
        /// <summary>
        /// Gets the Descriptors for use during Code Generation. CGR will invoke
        /// <see cref="ICodeGenerator{T}.GenerateAsync"/> in order to populate
        /// the Descriptors. Following which, will iterate the Descriptors in order
        /// to generate the subsequent code.
        /// </summary>
        ICollection<CodeGeneratorDescriptor> Descriptors { get; }
    }

    /// <summary>
    /// At its root Code Generation relays an enumeration of
    /// <see cref="CodeGeneratorDescriptor"/>. It is recommended to derive your
    /// Generators based on the <see cref="CodeGenerator{T}"/> base class.
    /// </summary>
    /// <inheritdoc cref="ICodeGenerator"/>
    public interface ICodeGenerator<in TContext> : ICodeGenerator
        where TContext : TransformationContextBase
    {
        /// <summary>
        /// Initiates the enumeration of your <see cref="CompilationUnitSyntax"/> with
        /// discoverable <paramref name="context"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="progress"></param>
        /// <param name="cancellationToken"></param>
        /// <see cref="TransformationContextBase"/>
        /// <returns></returns>
        Task GenerateAsync(TContext context, IProgress<Diagnostic> progress, CancellationToken cancellationToken);
    }
}
