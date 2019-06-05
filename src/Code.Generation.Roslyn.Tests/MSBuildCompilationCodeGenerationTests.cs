using System.IO;
using System.Threading.Tasks;
using Code.Generation.Roslyn.Integration;

namespace Code.Generation.Roslyn
{
    using Xunit;
    using Xunit.Abstractions;
    using static ModuleKind;

    public class MSBuildCompilationCodeGenerationTests : CompilationCodeGenerationTestFixtureBase<MSBuildCompilationManagerFixture>
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
        private Task OpenBundledProjectAsync()
        {
            return Task.Run(() =>
            {
                Bundle.Extrapolate(Bar | Baz | Biz | AssemblyInfo);
                CompilationManager.BuildWorkspace.OpenProjectAsync(Bundle.ProjectFileName).Wait();
            });
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
                    .AssertEqual(Path.GetFullPath(Bundle.ProjectFileName));

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
                CompilationManager.BuildWorkspace.CloseSolution();
                Bundle?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
