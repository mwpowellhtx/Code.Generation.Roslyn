using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.MSBuild;

namespace Code.Generation.Roslyn
{
    using Microsoft.CodeAnalysis;

    // TODO: TBD: could potentially even deliver this one as a separate package as well...
    // ReSharper disable once InconsistentNaming
    public abstract class MSBuildCompilationManager : CompilationManager
    {
        // ReSharper disable once RedundantEmptyObjectOrCollectionInitializer
        /// <summary>
        /// Override or Add <see cref="Workspace"/> Property items to the Manager.
        /// </summary>
        public virtual IDictionary<string, string> WorkspaceProperties { get; } = new Dictionary<string, string> { };

        private Lazy<Workspace> LazyWorkspace { get; }

        /// <inheritdoc />
        public override Workspace Workspace => LazyWorkspace.Value;

        /// <summary>
        /// Gets the <see cref="Workspace"/> in terms of a <see cref="MSBuildWorkspace"/>.
        /// </summary>
        public virtual MSBuildWorkspace BuildWorkspace => Workspace as MSBuildWorkspace;

        //protected override Solution Solution => base.Solution;

        protected MSBuildCompilationManager()
        {
            //// TODO: TBD: perhaps add test projects and member assets as embedded resources that we extrapolate and evaluate ...
            //// TODO: TBD: which can open solution, open projects, etc...
            //new MSBuildWorkspace()
            //new MSBuildWorkspace().OpenSolutionAsync("")

            // TODO: TBD: abstract this one and derive 1) MSBuild 2) Adhoc ...
            LazyWorkspace = new Lazy<Workspace>(() => MSBuildWorkspace.Create(WorkspaceProperties));

            // TODO: TBD: then what to do about "sources", never mind "solution", "projects", etc...
        }
    }
}