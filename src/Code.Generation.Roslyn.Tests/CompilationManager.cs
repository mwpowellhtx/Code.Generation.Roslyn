﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Code.Generation.Roslyn
{
    using Microsoft.CodeAnalysis.Text;
    using static LanguageNames;
    using static String;
    using static LanguageVersion;
    using static OutputKind;

    public abstract class CompilationManager : IDisposable
    {
        /// <summary>
        /// Returns a New <see cref="Guid"/> basis for the Asset Name. In this case,
        /// &quot;Asset&quot; may be a <see cref="Project"/> name, <see cref="Document"/> name,
        /// even <see cref="Solution"/> name, etc.
        /// </summary>
        /// <returns></returns>
        protected static string GetNewAssetName() => $"{Guid.NewGuid():D}";

        /// <summary>
        /// Gets the <see cref="Compilation"/> Language. Default is <see cref="CSharp"/>.
        /// </summary>
        /// <see cref="CSharp"/>
        protected virtual string Language => CSharp;

        // TODO: TBD: may allow VB? Personally, I do not care about VB, but someone might ...
        /// <summary>
        /// Gets the <see cref="Document"/> File Name Extension,
        /// which should correspond with the specified <see cref="Language"/>.
        /// </summary>
        protected virtual string LanguageDocumentExtension => ".cs";

        /// <summary>
        /// Gets the <see cref="Microsoft.CodeAnalysis.CompilationOptions.OutputKind"/>.
        /// Defaults to <see cref="DynamicallyLinkedLibrary"/>.
        /// </summary>
        protected virtual OutputKind CompilationOptionsOutputKind => DynamicallyLinkedLibrary;

        // TODO: TBD: may allow for language-specific derivatives... i.e. CSharpCompilationOptions.
        /// <summary>
        /// Gets the CompilationOptions.
        /// </summary>
        protected virtual CompilationOptions CompilationOptions =>
            new CSharpCompilationOptions(CompilationOptionsOutputKind);

        /// <summary>
        /// Gets the SpecificLanguageVersion. Defaults to <see cref="Default"/>.
        /// </summary>
        protected virtual LanguageVersion SpecificLanguageVersion => Default;

        // TODO: TBD: ditto CompilationOptions re: derivatives... i.e. CSharpParseOptions.
        /// <summary>
        /// Gets the ParseOptions.
        /// </summary>
        protected virtual ParseOptions ParseOptions
            => new CSharpParseOptions(SpecificLanguageVersion)
                .MergeAssets(PreprocessorSymbols.ToArray()
                    , (o, x) => o.WithPreprocessorSymbols(x), x => x.Any());

        // TODO: TBD: work of origin using this: "SOMETHING_ACTIVE";
        /// <summary>
        /// Gets any PreprocessorSymbols involved during the Compilation.
        /// </summary>
        protected virtual IEnumerable<string> PreprocessorSymbols
        {
            get { yield break; }
        }

        /// <summary>
        /// EvaluateCompilation event.
        /// </summary>
        public virtual event EventHandler<CompilationDiagnosticEventArgs> EvaluateCompilation;

        // TODO: TBD: may furnish Generic type derived from Compilation...
        // TODO: TBD: i.e. CSharpCompilation, but this must also align with the Language, etc...
        /// <summary>
        /// Event handler occurs when <see cref="EvaluateCompilation"/> is requested.
        /// </summary>
        /// <param name="project"></param>
        /// <param name="diagnosticFilter"></param>
        protected virtual void OnEvaluateCompilation(Project project, ICompilationDiagnosticFilter diagnosticFilter)
        {
            var e = new CompilationDiagnosticEventArgs(project, diagnosticFilter);
            EvaluateCompilation?.Invoke(this, e);
        }

        protected virtual ICompilationDiagnosticFilter CreateDiagnosticFilter(Compilation compilation)
        {
            return new BasicCompilationDiagnosticFilter(compilation);
        }

        /// <summary>
        /// Resolves the <see cref="Compilation"/> given <paramref name="project"/>, the ensuing
        /// <paramref name="compiling"/>, as well as constituent elements that informed the
        /// request.
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="sources"></param>
        /// <param name="project"></param>
        /// <param name="compiling"></param>
        /// <param name="cancellationToken"></param>
        [Obsolete("is the projectName/sources version really that necessary after all?")]
        protected virtual void ResolveCompilation(string projectName, IReadOnlyList<string> sources, Project project
            , Task<Compilation> compiling, CancellationToken cancellationToken = default)
        {
            // TODO: TBD: may need to reconsider Task<Compilation> in cases where Analyzers are involved, i.e. Task<CompilationWithAnalyzers> (?)
            OnEvaluateCompilation(project, CreateDiagnosticFilter(compiling.Result));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="project"></param>
        /// <param name="compiling"></param>
        /// <param name="cancellationToken"></param>
        protected virtual void ResolveCompilation(Project project, Task<Compilation> compiling, CancellationToken cancellationToken = default)
        {
            OnEvaluateCompilation(project, CreateDiagnosticFilter(compiling.Result));
        }

        /// <summary>
        /// Disposes the Object.
        /// </summary>
        /// <param name="disposing"></param>
        /// <see cref="Workspace"/>
        protected virtual void Dispose(bool disposing)
        {
        }

        /// <summary>
        /// Gets whether the Fixture IsDisposed.
        /// </summary>
        protected bool IsDisposed { get; private set; }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            IsDisposed = true;
        }
    }

    // TODO: TBD: may have a look at my `Kingdom.CodeAnalysis.Verifiers.Diagnostics' project from the Kingdom.Collections work space...
    // TODO: TBD: in particular leveraging methods such as GetMetadataReferenceFromType, GetTypeAssembly, etc...
    // TODO: TBD: could potentially be refactored, and further abstracted out at the same time, i.e. to not be as fit for purpose, i.e. concerning metadata references, etc.
    // TODO: TBD: additionally, strictly speaking, we do not necessarily need to connect such a package offering with the Xunit test framework. Strictly speaking.
    /// <summary>
    /// Establishes a basic CompilationManager for Roslyn based compilation services. We
    /// intentionally steer clear of introducing any in the way of things like test framework
    /// dependencies as these are really beyond the immediate purview of the compilation manager
    /// itself. We will allow sensible extensibility for this purpose if test framework level
    /// verification is so desired apart from the exposed <see cref="ResolveMetadataReferences"/>
    /// and <see cref="CompilationManager.EvaluateCompilation"/> events themselves.
    /// </summary>
    /// <inheritdoc />
    public abstract class CompilationManager<TWorkspace> : CompilationManager
        where TWorkspace : Workspace
    {
        // TODO: TBD: do I need to save project(s)/solution from being "adhoc" in order for this to play nicely with CG?
        // TODO: TBD: conversely, how might it be possible to CG using an in-memory Roslyn compilation?
        // TODO: TBD: because dotnet-cgr tooling runs in a process apart from the actual compilation...
        // TODO: TBD: I'm not sure that a compilation such as this would be able discover that output, not without help...

        /// <summary>
        /// Override in order to provide the <typeparamref name="TWorkspace"/> Factory.
        /// </summary>
        protected abstract Lazy<TWorkspace> LazyWorkspace { get; }

        /// <summary>
        /// Gets the Workspace involved during the Manager lifecycle.
        /// </summary>
        public virtual TWorkspace Workspace => LazyWorkspace.Value;

        /// <summary>
        /// Solution backing field.
        /// </summary>
        private Solution _solution;

        /// <summary>
        /// Gets the Solution involved during the Manager lifecycle. Starts from, or resets
        /// to, the <see cref="Microsoft.CodeAnalysis.Workspace.CurrentSolution"/>, depending
        /// on usage.
        /// </summary>
        /// <remarks>Privately Sets the Solution, especially during mutating operations.
        /// Which also prohibits us from being able to abstractly declare Solution at
        /// the base class level, as this would require the setter to be Protected,
        /// which we do not need to unnecessarily expose it to the rest of the class
        /// hierarchy.</remarks>
        public virtual Solution Solution
        {
            get => _solution ?? (_solution = Workspace.CurrentSolution);
            private set => _solution = value;
        }

        /// <summary>
        /// Occurs when it is time to Resolve the <see cref="Project.MetadataReferences"/>
        /// given the <see cref="Solution"/>.
        /// </summary>
        public virtual event EventHandler<ResolveMetadataReferencesEventArgs> ResolveMetadataReferences;

        /// <summary>
        /// Resolve the <see cref="Project.MetadataReferences"/> as furnished by
        /// <see cref="ResolveMetadataReferencesEventArgs.MetadataReferences"/>.
        /// </summary>
        /// <param name="solution">The <see cref="Solution"/> upon which the resolution is based.</param>
        /// <param name="project">The <see cref="Project"/> for which the References may occur.</param>
        /// <returns>A potentially modified <see cref="Solution"/> instance based upon <paramref name="solution"/>.</returns>
        protected virtual Solution OnResolveMetadataReferences(Solution solution, Project project)
        {
            var e = new ResolveMetadataReferencesEventArgs {Solution = solution, Project = project};
            ResolveMetadataReferences?.Invoke(this, e);
            // TODO: TBD: may report those references unable to add...
            return solution.MergeAssets(e.MetadataReferences.ToArray()
                , (g, x) => g.AddMetadataReferences(project.Id, x), x => x.Any());
        }

        /// <summary>
        /// Creates a New Project assuming <paramref name="projectName"/> and constituent member
        /// <paramref name="sources"/>.
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="sources"></param>
        /// <returns></returns>
        protected virtual Project CreateProjectFromSources(string projectName, params string[] sources)
        {
            projectName = IsNullOrEmpty(projectName) ? GetNewAssetName() : projectName;

            var projectId = ProjectId.CreateNewId(debugName: projectName);

            // We will assume C# as the language of choice for this purpose.
            Solution = Solution.AddProject(projectId, projectName, projectName, Language);

            // The flow is a bit inside-out, we need to have Added the Project first, which modifies the Solution.
            Solution = OnResolveMetadataReferences(Solution, Solution.GetProject(projectId))
                    .WithProjectCompilationOptions(projectId, CompilationOptions)
                    .WithProjectParseOptions(projectId, ParseOptions)
                ;

            sources.ToList().ForEach(src =>
            {
                var assetName = GetNewAssetName();
                var assetFileName = $"{assetName}{LanguageDocumentExtension}";
                // Adds the Document Connected with the Project to the Solution. Also about inside-out in my opinion.
                Solution = Solution.AddDocument(DocumentId.CreateNewId(projectId), assetFileName,
                    SourceText.From(src));
            });

            // Hold off getting the project because we need the most up to date state.
            return Solution.GetProject(projectId);
        }

        public virtual void CompileAllProjects()
        {
            // TODO: TBD: not counting build orders?
            Solution.Projects.ToList().ForEach(
                p => ResolveCompilation(
                    p
                    , p.WithCompilationOptions(CompilationOptions).WithParseOptions(ParseOptions).GetCompilationAsync()
                )
            );
        }

        /// <summary>
        /// Compiles the <paramref name="projectName"/> given constituent
        /// <paramref name="sources"/>, assuming the Manager configuration.
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="sources"></param>
        public virtual void Compile(string projectName, params string[] sources)
        {
            Project CreateProject(out Project project) => project = CreateProjectFromSources(projectName, sources);
            // TODO: TBD: expand upon the CancellationToken aspects...
            ResolveCompilation(projectName, sources, CreateProject(out var p), p.GetCompilationAsync());
        }

        /// <summary>
        /// Disposes the Object. In this instance we have a <see cref="Workspace"/> to Dispose.
        /// </summary>
        /// <param name="disposing"></param>
        /// <see cref="Workspace"/>
        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (IsDisposed || !disposing)
            {
                return;
            }

            Workspace?.Dispose();

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            // True, while always True, makes it clearer, Follow the Chain.
            base.Dispose(disposing);
        }
    }
}
