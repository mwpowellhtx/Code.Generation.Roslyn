namespace Code.Generation.Roslyn
{
    using Generators;
    using Xunit;
    using Xunit.Abstractions;
    using static Program;
    using static Generators.Integration.ModuleKind;

    public class ToolingTests : ToolingTestFixtureBase
    {
        public ToolingTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        [Fact]
        public void Single_CG_Pass_Generates_As_Expected()
        {
            // TODO: TBD: some amount of this is "fixturing" I think...
            // TODO: TBD: but how much of it truly?
            VerifyWithOperators(bundle =>
            {
                bundle.Extrapolate(Bar | Baz | Biz | Buz | AssemblyInfo);
                bundle.AddClassAnnotation<ImplementBuzInterfaceAttribute>(Bar);
            }, args =>
            {
                args.AddSources($@"{Bundle.GetFilePath(Bar)}");
                args.AddDefines(@"DEBUG");
                args.AddReferenceToTypeAssembly<object>();
                args.AddTypeAssemblyLocationBasedReferences<object>(@"netstandard.dll", @"System.Runtime.dll");
                args.AddReferenceToTypeAssembly<ImplementBuzInterfaceAttribute>();
            }).AssertEqual(DefaultErrorLevel);
        }
    }
}
