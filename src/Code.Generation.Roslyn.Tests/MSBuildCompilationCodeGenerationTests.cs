using System.IO;
using System.Threading.Tasks;
using Code.Generation.Roslyn.Integration;

namespace Code.Generation.Roslyn
{
    using Microsoft.CodeAnalysis.MSBuild;
    using Xunit;
    using Xunit.Abstractions;
    using static Path;
    using static ModuleKind;

    // ReSharper disable once InconsistentNaming
    public class MSBuildCompilationCodeGenerationTests : CompilationCodeGenerationTestFixtureBase<MSBuildWorkspace, MSBuildCompilationManagerFixture>
    {
        private TestCaseBundle Bundle { get; }

        public MSBuildCompilationCodeGenerationTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            Bundle = new TestCaseBundle();
        }

        /// <summary>
        /// We are not really here to test the Workspace functionality, per se. However,
        /// we do want to ensure that our environment can be aligned and ready to go.
        /// </summary>
        /// <returns></returns>
        private Task OpenBundledProjectAsync() => Task.Run(() =>
        {
            Bundle.Extrapolate(Bar | Baz | Biz | AssemblyInfo);
            CompilationManager.Workspace.OpenProjectAsync(Bundle.ProjectFileName).Wait();
        });

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
                Bundle?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
