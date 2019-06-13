using System.IO;
using System.Linq;
using System.Text;

namespace Code.Generation.Roslyn
{
    using Integration;
    using Xunit;
    using Xunit.Abstractions;
    using static Domain;
    using static Program;
    using static StringLiterals;

    /// <summary>
    /// The primary goal of these tests is to verify invocation of the Tooling. Short of
    /// invoking via Command Line, never mind wiring up some Microsoft Build Targets, this
    /// is about as comprehensive an end-to-end set of integration tests as there gets for
    /// purposes of verifying expected behavior. I think we even should be able to achieve
    /// scenarios as complicated as re-generating code based on updated source, etc. We can
    /// even verify expected Error Level codes during the process.
    /// </summary>
    public abstract class ToolingTestFixtureBase : TestFixtureBase
    {
        protected TestCaseBundle Bundle { get; } = new TestCaseBundle();

        protected ToolingTestFixtureBase(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        protected delegate int MainCallback(params string[] args);

        private static int DefaultMainCallback(params string[] args) => Program.Main(args);

        protected StringBuilder OutBuilder { get; private set; }

        /// <summary>
        /// Verifies via the <paramref name="callback"/> given <paramref name="args"/>.
        /// Returns the Error Level received after the <see cref="MainCallback"/>.
        /// </summary>
        /// <param name="callback">The Main Callback.
        /// Defaults to <see cref="DefaultMainCallback"/>.</param>
        /// <param name="args"></param>
        /// <returns></returns>
        protected virtual int Verify(MainCallback callback, params string[] args)
        {
            OutBuilder = new StringBuilder();
            Program.Out = new StringWriter(OutBuilder.AssertNotNull());
            return (callback ?? DefaultMainCallback).Invoke(args);
        }

        /// <summary>
        /// Verifies the <paramref name="args"/> assuming default <see cref="MainCallback"/>.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        protected virtual int Verify(params string[] args) => Verify(null, args);

        /// <summary>
        /// Operates on the <paramref name="bundle"/>.
        /// </summary>
        /// <param name="bundle"></param>
        internal delegate void TestCaseBundleOperator(TestCaseBundle bundle);

        /// <summary>
        /// Operates on the <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder"></param>
        internal delegate void ToolingParameterOperator(ToolingParameterBuilder builder);

        /// <summary>
        /// Renders the <see cref="OutBuilder"/> while also trimming any whitespace.
        /// </summary>
        /// <returns></returns>
        private string RenderOut() => $@"{OutBuilder}".Trim();

        [Fact]
        public void Verify_Version()
        {
            var errorLevelNotExpected = (int?) null;

            // ReSharper disable once ExpressionIsAlwaysNull
            var errorLevel = Verify($@"{DoubleDash}version").AssertEqual(1);

            // TODO: TBD: what else to verify/report here?
            OutputHelper.WriteLine($@"Tooling version is {RenderOut()} (Error Level: {errorLevel}).");
        }

        [Fact]
        public void Verify_No_Arguments_Specified()
        {
            if ((Verify() == 1).AssertTrue())
            {
                RenderOut().AssertEqual(NoSourceFilesSpecified);
            }
        }

        /// <summary>
        /// Pretty much encapsulates the Round Trip invocation of the <see cref="Program.Main"/>
        /// method exposed in the Code Generation Tooling. This is as comprehensive an integration
        /// test as there is short of calling out to the Command Line Process itself, never mind
        /// wiring up the MSBuild targets.
        /// </summary>
        /// <param name="bundleOperator"></param>
        /// <param name="parameterOperator"></param>
        /// <returns></returns>
        internal virtual int VerifyWithOperators(TestCaseBundleOperator bundleOperator, ToolingParameterOperator parameterOperator)
        {
            bundleOperator?.Invoke(Bundle);
            var builder = new ToolingParameterBuilder {Project = $@"{Bundle.ProjectName}"};
            parameterOperator?.Invoke(builder);
            return Verify(builder.ToArray());
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !IsDisposed)
            {
                Bundle.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
