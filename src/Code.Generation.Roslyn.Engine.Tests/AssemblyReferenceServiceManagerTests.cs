using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Code.Generation.Roslyn
{
    using Xunit;
    using Xunit.Abstractions;
    using static Resources;

    public class AssemblyReferenceServiceManagerTests : TestFixtureBase
    {
        public AssemblyReferenceServiceManagerTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        /// <summary>
        /// Gets the <see cref="AssemblyReferenceServiceManager.AllowedAssemblyExtensions"/>
        /// for Internal Use.
        /// </summary>
        /// <see cref="AssemblyReferenceServiceManager.AllowedAssemblyExtensions"/>
        private static HashSet<string> AllowedAssemblyExtensions => AssemblyReferenceServiceManager.AllowedAssemblyExtensions.AssertNotNull();

        [Fact]
        public void Assert_Allowed_Assembly_Extensions_Are_Correct()
        {
            var expected = GetRange(AllowedAssemblyExtensionDll);
            AllowedAssemblyExtensions.AssertNotEmpty().AssertEqual(expected);
        }

        /// <summary>
        /// &quot;Blah.Blah&quot;
        /// </summary>
        private const string BlahBlahAssemblyName = "Blah.Blah";

        /// <summary>
        /// &quot;Kingdom.Simple.Calculators&quot;
        /// </summary>
        private const string SubjectAssemblyName = "Kingdom.Simple.Calculators";

        // ReSharper disable once StringLiteralTypo relative from the anticipated working directory.
        /// <summary>
        /// Gets the SubjectSearchPath.
        /// Always expecting this in the specified Configuration and Target Framework.
        /// </summary>
        /// <see cref="SubjectAssemblyName"/>
        private static string SubjectSearchPath => $@"..\..\..\..\{SubjectAssemblyName}\bin\Debug\netstandard2.0";

        private static void VerifySearchMatchingAssemblyPath(IEnumerable<string> paths, Func<AssemblyName> factory, Action<string> verify)
        {
            var assemblyName = factory.Invoke().AssertNotNull();
            var matched = AssemblyReferenceServiceManager.GetSearchMatchingAssemblyPath(paths.ToArray(), assemblyName);
            verify.Invoke(matched);
        }

        private static void VerifyReferenceMatchingAssemblyPath(IEnumerable<string> paths, Func<AssemblyName> factory, Action<string> verify)
        {
            var assemblyName = factory.Invoke().AssertNotNull();
            var match = AssemblyReferenceServiceManager.GetMatchingReferenceAssemblyPath(paths.ToArray(), assemblyName);
            verify.Invoke(match);
        }

        // TODO: TBD: should also support Strong Names? i.e. "System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
        [Fact]
        public void Invalid_Search_Matching_Assembly_Path_Correct()
            => VerifySearchMatchingAssemblyPath(GetRange(SubjectSearchPath)
                , () => new AssemblyName(BlahBlahAssemblyName), match => match.AssertNull());

        [Fact]
        public void Search_Matching_Assembly_Path_Correct()
            => VerifySearchMatchingAssemblyPath(GetRange(SubjectSearchPath),
                () => new AssemblyName(SubjectAssemblyName), match => match.AssertNotNull().AssertFileExists());

        [Fact]
        public void Invalid_Reference_Matching_Assembly_Path_Correct()
            => VerifyReferenceMatchingAssemblyPath(
                GetRange(SubjectSearchPath.CombinePaths($"{SubjectAssemblyName}{AllowedAssemblyExtensionDll}"))
                , () => new AssemblyName(BlahBlahAssemblyName), match => match.AssertNull());

        [Fact]
        public void Reference_Matching_Assembly_Path_Correct()
            => VerifyReferenceMatchingAssemblyPath(
                GetRange(SubjectSearchPath.CombinePaths($"{SubjectAssemblyName}{AllowedAssemblyExtensionDll}"))
                , () => new AssemblyName(SubjectAssemblyName), match => match.AssertNotNull().AssertFileExists());

        // TODO: TBD: test the other aspects leading up to and including assembly loading...
    }
}
