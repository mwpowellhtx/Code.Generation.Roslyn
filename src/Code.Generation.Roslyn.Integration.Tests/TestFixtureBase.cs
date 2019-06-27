namespace Foo
{
    using Xunit.Abstractions;

    public abstract class TestFixtureBase
    {
        protected ITestOutputHelper OutputHelper { get; }

        protected TestFixtureBase(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper;
        }
    }
}
