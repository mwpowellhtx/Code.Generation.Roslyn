using System.Linq;

namespace Code.Generation.Roslyn
{
    /// <summary>
    /// Represents a <see cref="GeneratedSyntaxTreeRegistry"/> Data Transfer Object used
    /// during Json serialization.
    /// </summary>
    public class GeneratedSyntaxTreeRegistryTransferObject : DescriptorRegistryTransferObject<GeneratedSyntaxTreeDescriptor>
    {
        /// <summary>
        /// Protected Internal Default Constructor.
        /// </summary>
        /// <remarks>Allowing for derivations from the base Data Transfer Object concerns.</remarks>
        protected internal GeneratedSyntaxTreeRegistryTransferObject()
        {
        }

        /// <summary>
        /// Returns an implicitly converted <paramref name="registry"/> to
        /// <see cref="GeneratedSyntaxTreeRegistryTransferObject"/>.
        /// </summary>
        /// <param name="registry"></param>
        public static implicit operator GeneratedSyntaxTreeRegistryTransferObject(GeneratedSyntaxTreeRegistry registry)
        {
            var dto = new GeneratedSyntaxTreeRegistryTransferObject();
            registry.ToList().ForEach(dto.Descriptors.Add);
            return dto;
        }
    }
}
