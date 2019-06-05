// Copyright (c) Michael W. Powell. All rights reserved.
// Licensed under the GPPLv3 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Code.Generation.Roslyn
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    public abstract class CodeGeneratorBase : ICodeGenerator
    {
        // ReSharper disable once RedundantEmptyObjectOrCollectionInitializer
        protected virtual ICollection<CodeGeneratorDescriptor> Descriptors { get; } = new List<CodeGeneratorDescriptor> { };

        public IEnumerator<CodeGeneratorDescriptor> GetEnumerator() => Descriptors.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public abstract Task GenerateAsync(TransformationContext context, IProgress<Diagnostic> progress
            , CancellationToken cancellationToken);

        /// <summary>
        /// Finds the most recent <see cref="NamespaceDeclarationSyntax"/>.
        /// Such Declarations may be Nested. It is up to the Caller to discern
        /// what this means to the Code Generator.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected static IEnumerable<NamespaceDeclarationSyntax> FindNamespaceDeclarations(SyntaxNode node)
        {
            IEnumerable<T> SelectWhile<T>(T x, Func<T, bool> predicate, Func<T, T> transform)
            {
                bool mayContinue;
                do
                {
                    yield return x;
                    mayContinue = predicate(x);
                    x = transform(x);
                } while (mayContinue);
            }

            return SelectWhile(node, x => x.Parent != null, x => x.Parent)
                .Reverse().OfType<NamespaceDeclarationSyntax>();
        }
    }
}
