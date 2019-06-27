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
    public abstract class CloneableVerificationTestFixtureBase<T> : ClassVerificationTestFixtureBase<T, ICloneable>
        where T : class, ICloneable, new()
    {
        protected CloneableVerificationTestFixtureBase(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        [Fact]
        public void Verify_Cloneable()
        {
            var instance = Instance.AssertNotNull();
            instance.Clone().AssertNotSame(instance);
        }
    }
}
