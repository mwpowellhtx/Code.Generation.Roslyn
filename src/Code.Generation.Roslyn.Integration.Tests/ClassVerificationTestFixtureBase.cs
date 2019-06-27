namespace Foo
{
    using Xunit.Abstractions;

    public abstract class ClassVerificationTestFixtureBase<T, TBase> : TestFixtureBase
        where T : TBase, new()
        where TBase : class
    {
        /// <summary>
        /// Gets a New Instance of the <typeparamref name="T"/> Class.
        /// </summary>
        protected static T Instance => new T();

        protected ClassVerificationTestFixtureBase(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }
    }
}
