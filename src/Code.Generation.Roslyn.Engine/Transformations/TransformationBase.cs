using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Code.Generation.Roslyn
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Validation;

    public abstract class TransformationBase : ServiceManager
    {
        private string _projectDirectory;

        /// <summary>
        /// Gets the ProjectDirectory.
        /// </summary>
        internal string ProjectDirectory
        {
            get => _projectDirectory;
            set
            {
                Assumes.True(Directory.Exists(value), $"Directory `{value}´ does not exist.");
                _projectDirectory = value;
            }
        }

        /// <summary>
        /// Gets the ReferenceService.
        /// </summary>
        internal AssemblyReferenceServiceManager ReferenceService { get; }

        /// <summary>
        /// Protected Constructor.
        /// </summary>
        /// <param name="referenceService"></param>
        protected TransformationBase(AssemblyReferenceServiceManager referenceService)
        {
            Requires.NotNull(referenceService, nameof(referenceService));
            ReferenceService = referenceService;
        }

        /// <summary>
        /// Transforms the current <paramref name="compilation"/> in terms of a set of
        /// <see cref="CompilationUnitSyntax"/>.
        /// </summary>
        /// <param name="compilation"></param>
        /// <param name="progress"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <see cref="ReferenceService"/>
        public abstract Task<IEnumerable<CompilationUnitSyntax>> TransformAsync(CSharpCompilation compilation
            , IProgress<Diagnostic> progress, CancellationToken cancellationToken);
    }

    public abstract class TransformationBase<TContext, TTransformation> : TransformationBase
        where TContext : TransformationContextBase
        where TTransformation : TransformationBase<TContext, TTransformation>
    {
        /// <summary>
        /// Protected Constructor.
        /// </summary>
        /// <param name="referenceService"></param>
        /// <inheritdoc />
        protected TransformationBase(AssemblyReferenceServiceManager referenceService)
            : base(referenceService)
        {
        }

        /// <summary>
        /// Provides an opportunity to Configure the Transformation prior to continuing.
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public TTransformation Configure(Action<TTransformation> config = null)
        {
            config?.Invoke((TTransformation) this);
            return (TTransformation) this;
        }
    }
}
