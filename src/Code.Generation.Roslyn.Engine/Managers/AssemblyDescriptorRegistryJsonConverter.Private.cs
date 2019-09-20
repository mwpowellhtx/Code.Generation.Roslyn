namespace Code.Generation.Roslyn
{
    using Newtonsoft.Json;

    public partial class AssemblyDescriptorRegistryJsonConverter : JsonConverter<AssemblyDescriptorRegistry>
    {
        /// <summary>
        /// Private Default Constructor.
        /// </summary>
        private AssemblyDescriptorRegistryJsonConverter()
        {
        }

        /// <summary>
        /// Gets a new Converter instance.
        /// </summary>
        public static AssemblyDescriptorRegistryJsonConverter Converter => new AssemblyDescriptorRegistryJsonConverter();
    }
}
