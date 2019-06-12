namespace Code.Generation.Roslyn
{
    using Generators;
    using Xunit;
    using Xunit.Abstractions;
    using static Generators.Integration.ModuleKind;

    // ReSharper disable once InconsistentNaming
    public class MSBuildCompilationCodeGenerationTests : MSBuildCompilationCodeGenerationTestFixtureBase
    {
        public MSBuildCompilationCodeGenerationTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        [Fact]
        public async void Verify_That_Async()
        {
            await OpenBundledProjectAsync();

            Bundle.AddClassAnnotation<ImplementBuzInterfaceAttribute>(Bar);
            Bundle.AddOuterTypeNamespaceUsingStatement<ImplementBuzInterfaceAttribute>(Bar);

            ResolveCompilation();
        }
    }
}
