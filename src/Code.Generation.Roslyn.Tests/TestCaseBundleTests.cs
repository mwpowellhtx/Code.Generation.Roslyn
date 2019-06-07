using System;
using System.Collections.Generic;

namespace Code.Generation.Roslyn
{
    using Integration;
    using Xunit;
    using Xunit.Abstractions;
    using static Constants;
    using static Integration.ModuleKind;
    using AttributeRenderingOptionDictionary = Dictionary<string, object>;
    using IAttributeRenderingOptionDictionary = IDictionary<string, object>;

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="module"></param>
        /// <param name="shorthand"></param>
        /// <param name="fullName"></param>
        [Theory
         , InlineData(Bar, true, true)
         , InlineData(Bar, true, false)
         , InlineData(Bar, false, true)
         , InlineData(Bar, false, false)]
        public void Can_AddClassAttribute(ModuleKind module, bool shorthand, bool fullName)
        {
            Bundle.Extrapolate(Bar | Baz | Biz | AssemblyInfo);

            var options = GetAttributeRenderingOptions(shorthand, fullName);

            Bundle.AddClassAnnotation<ObsoleteAttribute>(module, options);
            Bundle.AddInnerTypeNamespaceUsingStatement<ObsoleteAttribute>(module);

            var moduleFilePath = Bundle.GetFilePath(module);

            moduleFilePath.AssertFileContains(this.RenderAttributeNotation<ObsoleteAttribute>(options));
            moduleFilePath.AssertFileContains($"{Using} {typeof(ObsoleteAttribute).Namespace}{SemiColon}");
        }

        [Theory
         , InlineData(Biz, true, true)
         , InlineData(Biz, true, false)
         , InlineData(Biz, false, true)
         , InlineData(Biz, false, false)]
        public void Can_AddInterfaceAttribute(ModuleKind module, bool shorthand, bool fullName)
        {
            Bundle.Extrapolate(Bar | Baz | Biz | AssemblyInfo);

            var options = GetAttributeRenderingOptions(shorthand, fullName);

            Bundle.AddInterfaceAnnotation<ObsoleteAttribute>(module, options);
            Bundle.AddOuterTypeNamespaceUsingStatement<ObsoleteAttribute>(module);

            var moduleFilePath = Bundle.GetFilePath(module);

            moduleFilePath.AssertFileContains(this.RenderAttributeNotation<ObsoleteAttribute>(options));
            moduleFilePath.AssertFileContains($"{Using} {typeof(ObsoleteAttribute).Namespace}{SemiColon}");
        }

        [Theory
         , InlineData(AssemblyInfo, true, true)
         , InlineData(AssemblyInfo, true, false)
         , InlineData(AssemblyInfo, false, true)
         , InlineData(AssemblyInfo, false, false)]
        public void Can_AddAssemblyAttribute(ModuleKind module, bool shorthand, bool fullName)
        {
            Bundle.Extrapolate(Bar | Baz | Biz | AssemblyInfo);

            var options = GetAttributeRenderingOptions(shorthand, fullName);

            Bundle.AddAssemblyAnnotation<ObsoleteAttribute>(module, options);

            var expected = this.RenderAttributeNotation<ObsoleteAttribute>(options);

            Bundle.GetFilePath(module).AssertFileContains(expected);
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
