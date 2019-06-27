using System;
using System.IO;

namespace Code.Generation.Roslyn
{
    using static Path;

    /// <summary>
    /// <see cref="ServiceManager"/> responsible for facilitating the Tool response
    /// to the Microsoft Build Clean target.
    /// </summary>
    /// <inheritdoc cref="ServiceManager"/>
    /// <remarks>The Clean target is a tough one to integration test, because, by definition,
    /// that would necessarily `Clean´ all of the integration project dependencies, starting
    /// with the Tool itself, extending through its dependencies, CGR, Engine, and so on.
    /// Which, the natural outcome of that is the project dependency outputs vanish. I think
    /// the best we can do with this one is to unit test the bits on the sandbox self-contained
    /// projects and see what happens. We will have to see how this works for purposes of
    /// deploying via package sources.</remarks>
    [Obsolete(ObsoleteMessage)]
    public class CleanServiceManager : ServiceManager, IDisposable
    {
        internal const string ObsoleteMessage
            = "Leaving this in for now, but will more than likely remove this eventually,"
              + " especially considering successful leveraging of internal targets triggered"
              + " by the Microsoft Build `Clean´ target.";

        /// <summary>
        /// Gets the GeneratedRegistryPath.
        /// </summary>
        private string GeneratedRegistryPath { get; }

        /// <summary>
        /// Gets the ReferenceRegistryPath.
        /// </summary>
        private string ReferenceRegistryPath { get; }

        /// <summary>
        /// Public Constructor.
        /// </summary>
        /// <param name="outputDirectory"></param>
        /// <param name="generatedRegistryFileName"></param>
        /// <param name="referenceRegistryFileName"></param>
        public CleanServiceManager(string outputDirectory, string generatedRegistryFileName
            , string referenceRegistryFileName)
        {
            GeneratedRegistryPath = Combine(outputDirectory, generatedRegistryFileName);
            ReferenceRegistryPath = Combine(outputDirectory, referenceRegistryFileName);
        }

        private static void RemoveRegistryAsset(string registryPath)
        {
            // We will start here with a `simple´ file removal.
            // TODO: TBD: as long as `Clean´ also removes `source´ files from the Output Directory, this should be fine, however...
            // TODO: TBD: we may need to consider actually loading the registr(y/ies) and doing a deeper removal, i.e. of the actual generated assets.
            if (!File.Exists(registryPath))
            {
                return;
            }

            File.Delete(registryPath);
        }

        /// <summary>
        /// Disposes of the Object.
        /// </summary>
        /// <param name="disposing">Whether the invocation is Disposing (true) or Finalizing (false).</param>
        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed || !disposing)
            {
                return;
            }

            RemoveRegistryAsset(GeneratedRegistryPath);
            RemoveRegistryAsset(ReferenceRegistryPath);
        }

        /// <summary>
        /// Indicates whether the Object IsDisposed.
        /// </summary>
        protected bool IsDisposed { get; private set; }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            IsDisposed = true;
        }
    }
}
