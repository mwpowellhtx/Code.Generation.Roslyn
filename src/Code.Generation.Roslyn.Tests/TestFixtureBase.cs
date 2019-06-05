using System;
using System.Collections.Generic;

namespace Code.Generation.Roslyn
{
    using Xunit.Abstractions;

    public abstract class TestFixtureBase : IDisposable
    {
        protected ITestOutputHelper OutputHelper { get; }

        protected TestFixtureBase(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper;
        }

        protected static IEnumerable<T> GetRange<T>(params T[] values)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var value in values)
            {
                yield return value;
            }
        }

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
