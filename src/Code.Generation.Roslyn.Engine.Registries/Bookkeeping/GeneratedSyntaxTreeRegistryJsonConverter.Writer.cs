using System.Collections.Generic;
using System.Linq;

namespace Code.Generation.Roslyn
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Descriptor = GeneratedSyntaxTreeDescriptor;

    public abstract partial class GeneratedSyntaxTreeRegistryJsonConverter<TRegistry>
        where TRegistry : GeneratedSyntaxTreeRegistry, new()
    {
        protected virtual JObject SerializeDescriptor(Descriptor descriptor)
            => new JObject(
                new JProperty(nameof(descriptor.SourceFilePath), descriptor.SourceFilePath)
                , new JProperty(nameof(descriptor.GeneratedAssetKeys)
                    , new JArray(descriptor.GeneratedAssetKeys.Select(y => $"{y:D}")))
            );

        /// <summary>
        /// Returns the Serialized <paramref name="descriptors"/>.
        /// </summary>
        /// <param name="descriptors"></param>
        /// <returns></returns>
        /// <see cref="Descriptor.GeneratedAssetKeys"/>
        protected virtual IEnumerable<JObject> SerializeDescriptors(IEnumerable<Descriptor> descriptors)
            => descriptors.Select(SerializeDescriptor);

        /// <summary>
        /// Serializes the <paramref name="registry"/> to <see cref="JObject"/>.
        /// </summary>
        /// <param name="registry"></param>
        /// <returns></returns>
        protected virtual JObject SerializeRegistry(TRegistry registry)
            => new JObject(
                new JProperty(nameof(registry.OutputDirectory), registry.OutputDirectory)
                , new JProperty(nameof(registry.Items), new JArray(SerializeDescriptors(registry.Items).ToArray<object>()))
            );

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, TRegistry registry, JsonSerializer serializer)
            => SerializeRegistry(registry).WriteTo(writer);
    }
}
