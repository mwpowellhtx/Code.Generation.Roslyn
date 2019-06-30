// Copyright (c) Michael W. Powell. All rights reserved.
// Licensed under the GPLv3 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace Code.Generation.Roslyn
{
    using Microsoft.CodeAnalysis;
    using Validation;

    public abstract class CodeGenerator : ICodeGenerator
    {
        /// <summary>
        /// Gets the AttributeData associated with the Request.
        /// </summary>
        protected AttributeData AttributeData { get; }

        /// <summary>
        /// Gets the Data Dictionary associated with the Request.
        /// </summary>
        protected ImmutableDictionary<string, TypedConstant> Data { get; }

        // ReSharper disable once RedundantEmptyObjectOrCollectionInitializer
        /// <inheritdoc />
        public virtual ICollection<CodeGeneratorDescriptor> Descriptors { get; }
            = new List<CodeGeneratorDescriptor> { };

        /// <inheritdoc />
        public IEnumerator<CodeGeneratorDescriptor> GetEnumerator() => Descriptors.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Protected Constructor.
        /// </summary>
        /// <param name="attributeData"></param>
        protected CodeGenerator(AttributeData attributeData)
        {
            Requires.NotNull(attributeData, nameof(attributeData));

            AttributeData = attributeData;

            Data = AttributeData.NamedArguments.ToImmutableDictionary(pair => pair.Key, pair => pair.Value);
        }
    }

    /// <summary>
    /// You may start with a brand new class hierarchy based solely upon
    /// <see cref="ICodeGenerator"/> if you want to, or you may leverage this base
    /// class, which furnishes many of the fundamental framework elements such as an
    /// <see cref="IEnumerable{T}"/> <see cref="ICodeGenerator.Descriptors"/>.
    /// </summary>
    /// <inheritdoc cref="ICodeGenerator{T}"/>
    public abstract class CodeGenerator<TContext> : CodeGenerator, ICodeGenerator<TContext>
        where TContext : TransformationContextBase
    {
        /// <inheritdoc />
        protected CodeGenerator(AttributeData attributeData)
            : base(attributeData)
        {
        }

        /// <inheritdoc />
        public abstract Task GenerateAsync(TContext context, IProgress<Diagnostic> progress, CancellationToken cancellationToken);
    }
}
