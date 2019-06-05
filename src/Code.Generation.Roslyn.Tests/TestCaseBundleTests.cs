using System;

namespace Code.Generation.Roslyn
{
    using Integration;
    using Xunit;
    using Xunit.Abstractions;
    using static Integration.ModuleKind;

    public class TestCaseBundleTests : TestFixtureBase
    {
        protected internal TestCaseBundle Bundle { get; }

        public TestCaseBundleTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            Bundle = new TestCaseBundle();
        }

        [Fact]
        public void Bundle_Id_Correct()
        {
            Bundle.Id.AssertNotEqual(Guid.Empty);
        }

        [Fact]
        public void Bundle_ProjectName_Correct()
        {
            if (Guid.TryParse(Bundle.ProjectName, out var actual).AssertTrue())
            {
                actual.AssertEqual(Bundle.Id);
            }

            // TODO: TBD: what else to do in an else/fail case?
        }

        [Theory
         , InlineData(null)
         , InlineData(Bar)
         , InlineData(Baz)
         , InlineData(Biz)
         , InlineData(AssemblyInfo)
         , InlineData(Bar | Baz)
         , InlineData(Bar | Baz | Biz)
         , InlineData(Bar | Baz | Biz | AssemblyInfo)]
        public void Bundle_Extrapolates(ModuleKind? modules)
        {
            Bundle.Extrapolate(modules);

            Bundle.GetFilePath($"{Bundle.ProjectName}.csproj").AssertFileExists();

            foreach (var module in modules.Distinct())
            {
                Bundle.GetFilePath(module).AssertFileExists();
            }
        }

        [Theory
         , InlineData(Bar)
         , InlineData(Baz)]
        public void Can_RemoveClassAttribute(ModuleKind module)
        {
            Bundle.Extrapolate(Bar | Baz | Biz | AssemblyInfo);

            Bundle.RemoveClassAnnotation(module);

            Bundle.GetFilePath(module).AssertFileDoesNotContain($"[{nameof(Attribute)}]");
        }

        [Theory
         , InlineData(Bar)
         , InlineData(Baz)]
        public void Can_RemoveClassAttribute_By_Generic(ModuleKind module)
        {
            Bundle.Extrapolate(Bar | Baz | Biz | AssemblyInfo);

            Bundle.RemoveClassAnnotation<Attribute>(module);

            Bundle.GetFilePath(module).AssertFileDoesNotContain($"[{nameof(Attribute)}]");
        }

        [Theory, InlineData(Biz)]
        public void Can_RemoveInterfaceAttribute(ModuleKind module)
        {
            Bundle.Extrapolate(Bar | Baz | Biz | AssemblyInfo);

            Bundle.RemoveInterfaceAnnotation(module);

            Bundle.GetFilePath(module).AssertFileDoesNotContain($"[{nameof(Attribute)}]");
        }

        [Theory, InlineData(Biz)]
        public void Can_RemoveInterfaceAttribute_By_Generic(ModuleKind module)
        {
            Bundle.Extrapolate(Bar | Baz | Biz | AssemblyInfo);

            Bundle.RemoveInterfaceAnnotation<Attribute>(module);

            Bundle.GetFilePath(module).AssertFileDoesNotContain($"[{nameof(Attribute)}]");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !IsDisposed)
            {
                Bundle?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
