namespace Foo
{
    using Xunit.Abstractions;

    public class CabVerificationTests : CloneableVerificationTestFixtureBase<Cab>
    {
        public CabVerificationTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }
    }
}
