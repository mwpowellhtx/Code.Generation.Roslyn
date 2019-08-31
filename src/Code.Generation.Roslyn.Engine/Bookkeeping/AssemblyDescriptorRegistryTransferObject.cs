using System.Linq;

namespace Code.Generation.Roslyn
{
    public class AssemblyDescriptorRegistryTransferObject : DescriptorRegistryTransferObject<AssemblyDescriptor>
    {
        /// <summary>
        /// Protected Internal Default Constructor.
        /// </summary>
        protected internal AssemblyDescriptorRegistryTransferObject()
        {
        }

        /// <summary>
        /// Returns an implicitly converted <paramref name="registry"/> to
        /// <see cref="AssemblyDescriptorRegistryTransferObject"/>.
        /// </summary>
        /// <param name="registry"></param>
        public static implicit operator AssemblyDescriptorRegistryTransferObject(AssemblyDescriptorRegistry registry)
        {
            var dto = new AssemblyDescriptorRegistryTransferObject();
            registry.ToList().ForEach(dto.Descriptors.Add);
            return dto;
        }
    }
}
