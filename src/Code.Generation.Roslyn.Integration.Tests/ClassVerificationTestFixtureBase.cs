namespace Foo
{
    using Xunit.Abstractions;

    public abstract class ClassVerificationTestFixtureBase<T, TBase> : TestFixtureBase
        where T : TBase, new()
        where TBase : class
    {
        protected T Instance => new T();

        protected ClassVerificationTestFixtureBase(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }
    }
}
