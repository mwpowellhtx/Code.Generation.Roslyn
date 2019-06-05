using System;
using System.Collections.Generic;

namespace Code.Generation.Roslyn
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.MSBuild;

    // TODO: TBD: could potentially even deliver this one as a separate package as well...
    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// This Manager derives for purposes of supporting Microsoft Build centric Compilation.
    /// </summary>
    /// <inheritdoc />
    public abstract class MSBuildCompilationManager : CompilationManager<MSBuildWorkspace>
    {
        /// <summary>
        /// Gets the <see cref="Lazy{T}"/> <see cref="MSBuildWorkspace"/> instance.
        /// </summary>
        /// <inheritdoc />
        protected override Lazy<MSBuildWorkspace> LazyWorkspace { get; }

        // ReSharper disable once RedundantEmptyObjectOrCollectionInitializer
        /// <summary>
        /// Override or Add <see cref="Workspace"/> Property items to the Manager.
        /// </summary>
        public virtual IDictionary<string, string> WorkspaceProperties { get; } = new Dictionary<string, string> { };

        //protected override Solution Solution => base.Solution;

        // ReSharper disable once RedundantEmptyObjectOrCollectionInitializer
        /// <summary>
        /// Default Constructor.
        /// </summary>
        protected MSBuildCompilationManager() : this(new Dictionary<string, string> { })
        {
        }

        protected MSBuildCompilationManager(IDictionary<string, string> workspaceProperties)
        {
            Initialize(workspaceProperties);

            LazyWorkspace = new Lazy<MSBuildWorkspace>(() => MSBuildWorkspace.Create(WorkspaceProperties));

            //// TODO: TBD: perhaps add test projects and member assets as embedded resources that we extrapolate and evaluate ...
            //// TODO: TBD: which can open solution, open projects, etc...
            //new MSBuildWorkspace()
            //new MSBuildWorkspace().OpenSolutionAsync("")

            // TODO: TBD: then what to do about "sources", never mind "solution", "projects", etc...
        }

        private void Initialize(IDictionary<string, string> workspaceProperties)
        {
            foreach (var wp in workspaceProperties)
            {
                WorkspaceProperties.Add(wp);
            }
        }
    }
}
