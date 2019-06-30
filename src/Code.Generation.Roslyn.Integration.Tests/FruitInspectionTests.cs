using System.Collections.Generic;
using System.Linq;

namespace Foo
{
    using Xunit;
    using Xunit.Abstractions;
    using static FruitKind;

    /// <summary>
    /// In keeping with the other Code Generation integration tests, first the code should
    /// build. Next, we should be able to verify some aspects of the generated code itself
    /// that are obvious and easy to identifier.
    /// </summary>
    public class FruitInspectionTests : EnumVerificationTestFixtureBase<FruitKind>
    {
        public FruitInspectionTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        /// <summary>
        /// Gets the <see cref="EnumVerificationTestFixtureBase{T}.Values"/>
        /// and verifies a couple of key aspects about that collection.
        /// </summary>
        protected override IEnumerable<FruitKind> Values => base.Values.AssertTrue(x => x.Count() == 6);

        /// <summary>
        /// Verifies that we have the Kind of Fruit we are expecting.
        /// </summary>
        /// <param name="actual"></param>
        [Theory
         , InlineData(Apple)
         , InlineData(Orange)
         , InlineData(Banana)
         , InlineData(Lime)
         , InlineData(Lemon)
         , InlineData(Cherry)]
        public void Verify_Kind(FruitKind actual) => Values.AssertContains(x => x == actual);
    }
}
