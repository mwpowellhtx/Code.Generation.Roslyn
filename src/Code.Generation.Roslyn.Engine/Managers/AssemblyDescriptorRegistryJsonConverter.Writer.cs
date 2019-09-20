using System.Collections.Generic;
using System.Linq;

namespace Code.Generation.Roslyn
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Descriptor = AssemblyDescriptor;
    using Registry = AssemblyDescriptorRegistry;

    public partial class AssemblyDescriptorRegistryJsonConverter
    {
        /// <summary>
        /// Returns the Serialized <paramref name="descriptor"/>.
        /// </summary>
        /// <param name="descriptor"></param>
        /// <returns></returns>
        protected virtual JObject SerializeDescriptor(Descriptor descriptor)
            => new JObject(
                new JProperty(nameof(descriptor.AssemblyPath), descriptor.AssemblyPath)
            );

        /// <summary>
        /// Returns the Serialized <paramref name="descriptors"/>.
        /// </summary>
        /// <param name="descriptors"></param>
        /// <returns></returns>
        protected virtual IEnumerable<JObject> SerializeDescriptors(IEnumerable<Descriptor> descriptors)
            => descriptors.Select(SerializeDescriptor);

        /// <summary>
        /// Serializes the <paramref name="registry"/> to <see cref="JObject"/>.
        /// </summary>
        /// <param name="registry"></param>
        /// <returns></returns>
        protected virtual JObject SerializeRegistry(Registry registry)
            => new JObject(
                new JProperty(nameof(registry.OutputDirectory), registry.OutputDirectory)
                , new JProperty(nameof(registry.Items), new JArray(SerializeDescriptors(registry).ToArray<object>()))
            );

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, Registry value, JsonSerializer serializer)
        {
            var @object = new JObject(
                new JProperty(nameof(value.OutputDirectory), value.OutputDirectory)
                , new JProperty(nameof(value.Items), new JArray(SerializeDescriptors(value).ToArray<object>()))
            );
            @object.WriteTo(writer);
        }
    }
}
