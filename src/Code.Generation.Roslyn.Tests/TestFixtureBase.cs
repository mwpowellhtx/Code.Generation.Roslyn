using System;
using System.Collections.Generic;
using System.Linq;

namespace Code.Generation.Roslyn
{
    using static Domain;
    using Xunit.Abstractions;
    using AttributeRenderingOptionDictionary = Dictionary<string, object>;
    using IAttributeRenderingOptionDictionary = IDictionary<string, object>;

    public abstract class TestFixtureBase : IDisposable
    {
        protected ITestOutputHelper OutputHelper { get; }

        /// <summary>
        /// Returns an <see cref="IAttributeRenderingOptionDictionary"/> instance given
        /// the <paramref name="shorthand"/> and <paramref name="fullName"/> arguments.
        /// </summary>
        /// <param name="shorthand"></param>
        /// <param name="fullName"></param>
        /// <returns></returns>
        protected static IAttributeRenderingOptionDictionary GetAttributeRenderingOptions(bool shorthand, bool fullName)
            => new AttributeRenderingOptionDictionary {{nameof(shorthand), shorthand}, {full_name, fullName}};

        protected TestFixtureBase(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper;
        }

        /// <summary>
        /// Yields a new Range of <paramref name="values"/> based on the arguments.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <returns></returns>
        protected static IEnumerable<T> GetRange<T>(params T[] values)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var value in values)
            {
                yield return value;
            }
        }

        protected static T[] GetRangeArray<T>(params T[] values) => GetRange(values).ToArray();

        protected virtual void Dispose(bool disposing)
        {
        }

        /// <summary>
        /// Gets whether IsDisposed.
        /// </summary>
        protected bool IsDisposed { get; private set; }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                Dispose(true);
            }

            IsDisposed = true;
        }
    }
}
