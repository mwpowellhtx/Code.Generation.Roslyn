namespace Foo
{
    using Xunit;
    using Xunit.Abstractions;

    public class CubVerificationTests : DisposableVerificationTestFixtureBase<Cub>
    {
        public CubVerificationTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        protected override Cub VerifyDisposable(Cub instance)
        {
            var y = base.VerifyDisposable(instance.AssertFalse(x => x.InternalIsDisposed));
            return y.AssertTrue(x => x.InternalIsDisposed);
        }
    }
}
