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
        public void Can_Generate_Code_Using_Microsoft_Build_Compilation()
        {
            OpenBundledProjectAsync().Wait();

            Bundle.AddClassAnnotation<ImplementBuzInterfaceAttribute>(Bar);
            Bundle.AddOuterTypeNamespaceUsingStatement<ImplementBuzInterfaceAttribute>(Bar);

            ResolveCompilation();
        }
    }
}
