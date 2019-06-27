using System;

namespace Foo
{
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// The fact that this builds successfully in the Integration Test project is evidence
    /// enough. We will do a brief fact check of that issue just the same, although, at that
    /// point, the result would speak for itself.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <inheritdoc cref="ClassVerificationTestFixtureBase{T,TBase}"/>
    public abstract class DisposableVerificationTestFixtureBase<T> : ClassVerificationTestFixtureBase<T, IDisposable>, IDisposable
        where T : class, IDisposable, new()
    {
        protected DisposableVerificationTestFixtureBase(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        /// <summary>
        /// Verifies the <paramref name="instance"/> <see cref="IDisposable"/>.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        protected virtual T VerifyDisposable(T instance)
        {
            instance.Dispose();
            return instance;
        }

        [Fact]
        public void Verify_Disposable() => VerifyDisposable(Instance.AssertNotNull());

        protected virtual void Dispose(bool disposing) => Instance.AssertNotNull().Dispose();

        protected bool IsDisposed { get; private set; }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            IsDisposed = true;
        }
    }
}
