using System;

namespace Code.Generation.Roslyn.Integration
{
    using static Constants;
    using static Resources;
    using static ModuleKind;

    public abstract class TestCaseFixtureBase : IDisposable
    {
        /// <summary>
        /// &quot;namespace Foo&quot;
        /// </summary>
        /// <see cref="Namespace"/>
        /// <see cref="Foo"/>
        protected static string FooNamespace => $"{Namespace} {Foo}";

        /// <summary>
        /// &quot;public partial class&quot;
        /// </summary>
        protected const string PublicPartialClass = "public partial class";

        /// <summary>
        /// &quot;public partial interface&quot;
        /// </summary>
        protected const string PublicPartialInterface = "public partial interface";

        protected Type FixtureType { get; }

        protected TestCaseFixtureBase()
        {
            FixtureType = GetType();
        }

        protected virtual string GetBundledResourcePath(string resourceName) => Combine(FixtureType.Namespace, resourceName);

        protected string GetFileName(ModuleKind module)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (module)
            {
                case Bar:
                case Baz:
                case AssemblyInfo:
                    return $"{module}.cs";

                case Biz:
                    return $"I{module}.cs";
            }

            return null;
        }

        public delegate string AttributeAnnotationCallback(string s);

        protected virtual void Dispose(bool disposing)
        {
        }

        protected bool IsDisposed { get; private set; }

        public void Dispose()
        {
            Dispose(true);
            IsDisposed = true;
        }
    }
}
