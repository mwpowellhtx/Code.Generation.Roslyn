using System.Collections.Generic;
using System.Linq;

namespace Code.Generation.Roslyn
{
    public class EligibleSyntaxTreeRegistry : PurgingSyntaxTreeRegistry<
        GeneratedSyntaxTreeDescriptor, GeneratedSyntaxTreeDescriptorComparer>
    {
        /// <summary>
        /// Gets a Default Comparer.
        /// </summary>
        /// <see cref="GeneratedSyntaxTreeDescriptorComparer.Comparer"/>
        private static GeneratedSyntaxTreeDescriptorComparer DefaultComparer
            => GeneratedSyntaxTreeDescriptorComparer.Comparer;

        /// <summary>
        /// Default Public Constructor.
        /// </summary>
        /// <inheritdoc />
        public EligibleSyntaxTreeRegistry()
            : base(DefaultComparer)
        {
        }

        /// <summary>
        /// Internal Constructor. This Constructor also affords an opportunity to relay
        /// the <see cref="IRegistrySet.OutputDirectory"/>.
        /// </summary>
        /// <param name="registrySet"></param>
        /// <inheritdoc />
        public EligibleSyntaxTreeRegistry(IEnumerable<GeneratedSyntaxTreeDescriptor> registrySet)
            : base(registrySet, DefaultComparer)
        {
        }

        /// <summary>
        /// Returns an implicitly converted <paramref name="registry"/>
        /// to <see cref="EligibleSyntaxTreeRegistry"/>.
        /// </summary>
        /// <param name="registry"></param>
        public static implicit operator EligibleSyntaxTreeRegistry(GeneratedSyntaxTreeRegistry registry)
        {
            var result = new EligibleSyntaxTreeRegistry {OutputDirectory = registry.OutputDirectory};
            // With a little help from an LF for List ForEach friendly Add.
            void Add(GeneratedSyntaxTreeDescriptor x) => result.Add(x);
            registry.ToList().ForEach(Add);
            return result;
        }
    }
}
