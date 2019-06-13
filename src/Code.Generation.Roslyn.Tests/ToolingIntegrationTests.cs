using System.IO;
using System.Linq;

namespace Code.Generation.Roslyn
{
    using Generators;
    using Xunit;
    using Xunit.Abstractions;
    using static Path;
    using static Program;
    using static Generators.Integration.ModuleKind;

    public class ToolingIntegrationTests : ToolingIntegrationTestFixtureBase
    {
        public ToolingIntegrationTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        [Fact]
        public void Single_CG_Pass_Generates_As_Expected()
        {
            // TODO: TBD: some amount of this is "fixturing" I think...
            // TODO: TBD: but how much of it truly?
            VerifyWithOperators(
                bundle =>
                {
                    bundle.Extrapolate(Bar | Baz | Biz | Buz | AssemblyInfo);
                    bundle.AddClassAnnotation<ImplementBuzInterfaceAttribute>(Bar);
                }
                , args =>
                {
                    args.AddSources($"{Bundle.GetFilePath(Bar)}");
                    args.AddDefines("DEBUG");
                    args.AddReferenceToTypeAssembly<object>();
                    args.AddTypeAssemblyLocationBasedReferences<object>("netstandard.dll", "System.Runtime.dll");
                    args.AddReferenceToTypeAssembly<ImplementBuzInterfaceAttribute>();
                }
                , out var registrySet).AssertEqual(DefaultErrorLevel);

            var expectedOutputDirectory = Combine(Bundle.ProjectName, "obj");

            // TODO: TBD: it is probably fair to say this should be the case ALWAYS, regardless of the scenario.
            registrySet.AssertNotNull().OutputDirectory.AssertNotNull().AssertEqual(expectedOutputDirectory);

            registrySet.AssertCollection(
                x =>
                {
                    var sourceLastWritten = File.GetLastWriteTimeUtc(x.SourceFilePath.AssertFileExists());

                    var generatedPaths = x.GeneratedAssetKeys
                        .Select(y => $"{Combine(expectedOutputDirectory, $"{y:D}.g.cs").AssertFileExists()}")
                        .ToArray();

                    var allGeneratedLastWritten = generatedPaths.Select(File.GetLastWriteTimeUtc).ToArray();
                    allGeneratedLastWritten.All(y => y >= sourceLastWritten).AssertTrue();
                }
            );
        }
    }
}
