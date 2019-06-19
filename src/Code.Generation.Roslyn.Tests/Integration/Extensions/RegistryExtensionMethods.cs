using System.IO;
using System.Linq;

namespace Code.Generation.Roslyn
{
    using Xunit;
    using static Path;

    internal static class RegistryExtensionMethods
    {
        // TODO: TBD: we think that this must be the case after every pass...
        // TODO: TBD: that is to say, we must be able to verify the integrity of the CG registries after each CG pass.
        public static GeneratedSyntaxTreeRegistry VerifyRegistry(this GeneratedSyntaxTreeRegistry actualRegistry
            , string expectedOutputDirectory, int? expectedCount = null)
        {
            // Not so much Asserting the Collection, as it is leaving it potentially Open Ended.
            void VerifyGenerated(GeneratedSyntaxTreeDescriptor x)
            {
                var sourceLastWritten = File.GetLastWriteTimeUtc(x.SourceFilePath.AssertFileExists());

                var generatedPaths = x.GeneratedAssetKeys
                    .Select(y => $"{Combine(expectedOutputDirectory, $"{y:D}.g.cs").AssertFileExists()}")
                    .ToArray();

                var allGeneratedLastWritten = generatedPaths.Select(File.GetLastWriteTimeUtc).ToArray();
                allGeneratedLastWritten.All(y => y >= sourceLastWritten).AssertTrue();
            }

            // TODO: TBD: it is probably fair to say this should be the case ALWAYS, regardless of the scenario.
            actualRegistry.AssertNotNull().OutputDirectory.AssertNotNull().AssertEqual(expectedOutputDirectory);

            if (expectedCount.HasValue)
            {
                actualRegistry.Count.AssertEqual(expectedCount.Value);
            }

            actualRegistry.ToList().ForEach(VerifyGenerated);

            return actualRegistry;
        }

        public static GeneratedSyntaxTreeRegistry VerifyResponseFile(
            this GeneratedSyntaxTreeRegistry expectedRegistry
            , string expectedOutputDirectory, string expectedResponsePath)
        {
            var actualPaths = File.ReadLines(expectedResponsePath.AssertFileExists()).ToArray();

            string CombineBaseDirectory(string fileName) => Combine(expectedOutputDirectory, fileName);

            var expectedPaths = expectedRegistry.SelectMany(d => d.GeneratedAssetKeys.Select(x => $"{x:D}.g.cs"))
                .Select(CombineBaseDirectory).ToArray();

            // ReSharper disable CommentTypo
            // Make sure that the Actuals are Actuals, same for Expected.
            actualPaths.Length.AssertEqual(expectedPaths.Length);

            // In no particular order so long as all of the Paths are present and accounted for.
            foreach (var expectedPath in expectedPaths)
            {
                actualPaths.AssertContainsAll(expectedPath);
            }
            // ReSharper restore CommentTypo

            return expectedRegistry;
        }
    }
}
