using System.Linq;

namespace Code.Generation.Roslyn
{
    using Generators.Integration;
    using Generators;
    using Xunit;
    using Xunit.Abstractions;
    using static Program;
    using static Generators.Integration.ModuleKind;

    public class ToolingIntegrationTests : ToolingIntegrationTestFixtureBase
    {
        public ToolingIntegrationTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        /// <summary>
        /// false
        /// </summary>
        private const bool DoNotUseResponseFile = false;

        private void VerifySinglePass(ModuleKind modules, ModuleKind module
            , string expectedOutputDirectory, string expectedResponsePath
            , out GeneratedSyntaxTreeRegistry registrySet
            , bool usingResponseFile = DoNotUseResponseFile)
        {
            // Do a little Fact checking of the parameters.
            modules.Distinct().ToArray().AssertTrue(x => x.Length > 1);
            module.Distinct().ToArray().AssertTrue(x => x.Length == 1);

            // TODO: TBD: some amount of this is "fixturing" I think...
            // TODO: TBD: but how much of it truly?
            VerifyWithOperators(
                bundle =>
                {
                    bundle.Extrapolate(modules);
                    bundle.AddClassAnnotation<ImplementBuzInterfaceAttribute>(module);
                }
                , args =>
                {
                    args.AddSources($"{Bundle.GetFilePath(module)}");
                    args.AddDefines("DEBUG");
                    if (usingResponseFile)
                    {
                        args.UsingResponseFile = true;
                    }

                    // And do an immediate verification that we set it correctly.
                    args.UsingResponseFile.AssertTrue(x => x == usingResponseFile);
                }
                , out registrySet).AssertEqual(DefaultErrorLevel);

            registrySet
                .VerifyRegistry(expectedOutputDirectory, 1)
                .VerifyResponseFile(expectedOutputDirectory, expectedResponsePath)
                ;
        }

        private void VerifySecondPass(ModuleKind module, ModuleKind modules
            , string expectedOutputDirectory, string expectedResponsePath
            , out GeneratedSyntaxTreeRegistry registrySet
            , bool usingResponseFile = DoNotUseResponseFile)
        {
            // Do a little Fact checking of the parameters.
            module.Distinct().ToArray().AssertTrue(x => x.Length == 1);
            modules.Distinct().ToArray().AssertTrue(x => x.Length >= 0);

            VerifyWithOperators(
                bundle => bundle.AddClassAnnotation<ImplementCloneableInterfaceAttribute>(module)
                , args =>
                {
                    /* Should add all of the modules that would ordinarily be discovered
                     * on the heels of analyzing for the Code Generation Attribute(s). */

                    args.AddSources($"{Bundle.GetFilePath(module)}");
                    args.AddSources(modules.Distinct().Select(x => $"{Bundle.GetFilePath(x)}").ToArray());

                    args.AddDefines("DEBUG");

                    if (usingResponseFile)
                    {
                        args.UsingResponseFile = true;
                    }

                    // Ditto immediate verification.
                    args.UsingResponseFile.AssertTrue(x => x == usingResponseFile);
                }
                , out registrySet).AssertEqual(DefaultErrorLevel);

            registrySet
                .VerifyRegistry(expectedOutputDirectory, 2)
                .VerifyResponseFile(expectedOutputDirectory, expectedResponsePath)
                ;
        }

        [Fact]
        public void Single_CG_Pass_Generates_Correctly()
            => VerifySinglePass(Bar | Baz | Biz | Buz | AssemblyInfo, Bar
                , ExpectedOutputDirectory, ExpectedResponsePath, out _);

        [Fact]
        public void Multiple_CG_Pass_Generates_Delta_Only()
        {
            var registrySets = GetRange<GeneratedSyntaxTreeRegistry>(null, null).ToArray();

            var expectedOutputDirectory = ExpectedOutputDirectory;
            var expectedResponsePath = ExpectedResponsePath;

            // Establish the first pass.
            VerifySinglePass(Bar | Baz | Biz | Buz | AssemblyInfo, Bar
                , expectedOutputDirectory, expectedResponsePath, out registrySets[0]);

            registrySets[0].AssertNotNull();

            // Then we engage with a second, different pass.
            VerifySecondPass(Baz, Bar, expectedOutputDirectory, expectedResponsePath, out registrySets[1]);
        }

        /// <summary>
        /// true
        /// </summary>
        private const bool UseResponseFile = true;

        /// <summary>
        /// This test gets even closer to a full end-to-end integration involving Microsoft
        /// Build Targets, or even simply Command Line, Tooling invocation.
        /// </summary>
        [Fact]
        public void Single_CG_Pass_Generates_Correctly_Using_Response_File()
            => VerifySinglePass(Bar | Baz | Biz | Buz | AssemblyInfo, Bar
                , ExpectedOutputDirectory, ExpectedResponsePath, out _, UseResponseFile);

        /// <summary>
        /// This test gets even closer to a full end-to-end integration involving Microsoft
        /// Build Targets, or even simply Command Line, Tooling invocation.
        /// </summary>
        [Fact]
        public void Multiple_CG_Pass_Generates_Delta_Only_Using_Response_Files()
        {
            var registrySets = GetRange<GeneratedSyntaxTreeRegistry>(null, null).ToArray();

            var expectedOutputDirectory = ExpectedOutputDirectory;
            var expectedResponsePath = ExpectedResponsePath;

            // Establish the first pass.
            VerifySinglePass(Bar | Baz | Biz | Buz | AssemblyInfo, Bar
                , expectedOutputDirectory, expectedResponsePath, out registrySets[0], UseResponseFile);

            registrySets[0].AssertNotNull();

            // Then we engage with a second, different pass.
            VerifySecondPass(Baz, Bar, expectedOutputDirectory, expectedResponsePath
                , out registrySets[1], UseResponseFile);
        }
    }
}
