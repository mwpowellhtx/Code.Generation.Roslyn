using System.IO;
using System.Threading.Tasks;

namespace Code.Generation.Roslyn
{
    using Integration;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.MSBuild;
    using Xunit;
    using Xunit.Abstractions;
    using static Path;
    using static Generators.Integration.ModuleKind;

    // ReSharper disable once InconsistentNaming
    public abstract class MSBuildCompilationCodeGenerationTestFixtureBase : CompilationCodeGenerationTestFixtureBase<MSBuildWorkspace, MSBuildCompilationManagerFixture>
    {
        protected TestCaseBundle Bundle { get; }

        protected MSBuildCompilationCodeGenerationTestFixtureBase(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            Bundle = new TestCaseBundle();
        }

        protected override void CompilationManager_OnEvaluateCompilation(object sender, CompilationDiagnosticEventArgs e)
        {
            base.CompilationManager_OnEvaluateCompilation(sender, e);

            var compilation = e.GetCompilation<CSharpCompilation>().AssertNotNull();
        }

        /// <summary>
        /// We are not really here to test the Workspace functionality, per se. However,
        /// we do want to ensure that our environment can be aligned and ready to go.
        /// </summary>
        /// <returns></returns>
        protected Task OpenBundledProjectAsync() => Task.Run(() =>
        {
            Bundle.Extrapolate(Bar | Baz | Biz | Buz | AssemblyInfo);
            CompilationManager.Workspace.OpenProjectAsync(Bundle.ProjectFileName).Wait();
        });

        protected virtual void ResolveCompilation()
        {
            // TODO: TBD: then what? and/or doing what else leading up to actual compilation?
            CompilationManager.CompileAllProjects();
        }

        /// <summary>
        /// Before we can Build anything, we need to know whether we are even Opening
        /// a Solution properly.
        /// </summary>
        [Fact]
        public async void Can_Open_Project_Async()
        {
            await OpenBundledProjectAsync();

            void VerifyPath(string path)
                => path.AssertEndsWith(Bundle.ProjectFileName)
                    .AssertEqual(GetFullPath(Bundle.ProjectFileName));

            // Expecting the One Project.
            CompilationManager.Solution.Projects.AssertCollection(
                p => VerifyPath(p.FilePath)
            );
        }

        // TODO: TBD: next... let's review some project artifacts, what kind of assembly references we might be able to make, refer to what targets, define what project properties, etc...

        protected override void Dispose(bool disposing)
        {
            if (disposing && !IsDisposed)
            {
                CompilationManager.Workspace.CloseSolution();
                Bundle.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
