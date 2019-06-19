namespace Code.Generation.Roslyn
{
    public class EligibleSyntaxTreeRegistry : PurgingSyntaxTreeRegistry<GeneratedSyntaxTreeDescriptor, GeneratedSyntaxTreeDescriptorComparer>
    {
        /// <summary>
        /// Gets a Default Comparer.
        /// </summary>
        private static GeneratedSyntaxTreeDescriptorComparer DefaultComparer => GeneratedSyntaxTreeDescriptorComparer.Comparer;

        /// <summary>
        /// Default Public Constructor.
        /// </summary>
        /// <inheritdoc />
        public EligibleSyntaxTreeRegistry() : base(DefaultComparer) { }

        /// <summary>
        /// Internal Constructor. This Constructor also affords an opportunity to relay
        /// the <see cref="IRegistrySet.OutputDirectory"/>.
        /// </summary>
        /// <param name="registrySet"></param>
        /// <inheritdoc />
        internal EligibleSyntaxTreeRegistry(IRegistrySet<GeneratedSyntaxTreeDescriptor> registrySet) : base(registrySet, DefaultComparer) { }
    }
}
