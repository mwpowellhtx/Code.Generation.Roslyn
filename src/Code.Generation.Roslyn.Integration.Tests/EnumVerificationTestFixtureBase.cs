using System;
using System.Collections.Generic;

namespace Foo
{
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Performs some basic verification on <see cref="Enum"/> oriented types.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class EnumVerificationTestFixtureBase<T> : TestFixtureBase
        where T : struct
    {
        /// <summary>
        /// The Enumerated <see cref="Type"/>.
        /// </summary>
        protected Type EnumType { get; } = typeof(T);

        protected EnumVerificationTestFixtureBase(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        /// <summary>
        /// Verifies that the Type is in fact set.
        /// </summary>
        [Fact]
        public void Type_Is_Set() => EnumType.AssertNotNull();

        /// <summary>
        /// Asserts that the <see cref="Type.IsEnum"/>.
        /// </summary>
        [Fact]
        public void Type_Is_Enum() => EnumType.AssertNotNull().AssertTrue(x => x.IsEnum);

        /// <summary>
        /// Gets the Values.
        /// </summary>
        protected virtual IEnumerable<T> Values
        {
            get
            {
                Type_Is_Enum();

                foreach (T x in Enum.GetValues(EnumType))
                {
                    yield return x;
                }
            }
        }
    }
}
