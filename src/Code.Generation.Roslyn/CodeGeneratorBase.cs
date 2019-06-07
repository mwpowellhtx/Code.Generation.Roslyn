// Copyright (c) Michael W. Powell. All rights reserved.
// Licensed under the GPPLv3 license. See LICENSE file in the project root for full license information.

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

    // TODO: TBD: refactor this one to the Code.Generation.Roslyn project...
    /// <summary>
    /// You may start with a brand new class hierarchy based solely upon
    /// <see cref="ICodeGenerator"/> if you want to, or you may leverage this base
    /// class, which furnishes many of the fundamental framework elements such as an
    /// <see cref="IEnumerable{T}"/> <see cref="Descriptors"/>.
    /// </summary>
    /// <inheritdoc />
    public abstract class CodeGeneratorBase : ICodeGenerator
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
        /// <summary>
        /// The purpose of your <see cref="GenerateAsync"/> is to populate the Descriptors.
        /// The framework scaffolding will iterate the <see cref="ICodeGenerator"/> as though
        /// it were an <see cref="IEnumerable{T}"/> of <see cref="CodeGeneratorDescriptor"/>
        /// instances, which it is, when your Code Generation has completely run its course.
        /// </summary>
        protected virtual ICollection<CodeGeneratorDescriptor> Descriptors { get; } = new List<CodeGeneratorDescriptor> { };

        public IEnumerator<CodeGeneratorDescriptor> GetEnumerator() => Descriptors.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Protected Constructor.
        /// </summary>
        /// <param name="attributeData"></param>
        protected CodeGeneratorBase(AttributeData attributeData)
        {
            //// TODO: TBD: do not know if this necessarily justifies a dependency on Validation ...
            //// TODO: TBD: it could prove a useful dependency, but will need to discuss it further...
            //Requires.NotNull(attributeData, nameof(attributeData));

            AttributeData = attributeData;

            Data = AttributeData.NamedArguments.ToImmutableDictionary(pair => pair.Key, pair => pair.Value);
        }

        /// <inheritdoc />
        public abstract Task GenerateAsync(TransformationContext context, IProgress<Diagnostic> progress, CancellationToken cancellationToken);
    }
}
