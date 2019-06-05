using System;
using System.Collections.Generic;

namespace Code.Generation.Roslyn.Integration
{
    // TODO: TBD: I could "bundle" these, but actually, I am chewing on whether this shouldn't just be a Theory with accompanying test cases...
    // TODO: TBD: that knows how to resolve its compilation in a self-contained Guid-driven manner...
    // TODO: TBD: the key will be what to do about references to things such as the Attributes/Generators project(s)...
    public class IntegrationTestCaseBundle
    {
        /// <summary>
        /// Gets the BundleId.
        /// </summary>
        internal Guid BundleId { get; } = Guid.NewGuid();

        /// <summary>
        /// &quot;Project.Template.xml&quot;
        /// </summary>
        protected const string ProjectXmlResourceName = "Project.Template.xml";

        /// <summary>
        /// Gets the DirectoryName based upon the <see cref="BundleId"/>.
        /// </summary>
        private string DirectoryName => $"{BundleId:D}";

        /// <summary>
        /// Gets the ProjectFileName starting with <see cref="BundleId"/>.
        /// </summary>
        private string ProjectFileName => $"{BundleId:D}.csproj";

        // ReSharper disable once RedundantEmptyObjectOrCollectionInitializer
        /// <summary>
        /// Gets the ResourceSourceFiles.
        /// </summary>
        internal ICollection<string> RequiredSourceFiles { get; } = new List<string> { };

        /// <summary>
        /// Internal Default Constructor.
        /// </summary>
        internal IntegrationTestCaseBundle()
        {
        }
    }
}
