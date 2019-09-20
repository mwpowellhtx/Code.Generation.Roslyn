using System;
using System.Collections.Generic;
using System.Linq;

namespace Code.Generation.Roslyn
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Descriptor = GeneratedSyntaxTreeDescriptor;

    // TODO: TBD: should we assume a Validation dependency? Fluently Validate?
    public abstract partial class GeneratedSyntaxTreeRegistryJsonConverter<TRegistry>
        where TRegistry : GeneratedSyntaxTreeRegistry, new()
    {
        /// <summary>
        /// Returns the Deserialized <see cref="Descriptor"/> <paramref name="object"/>.
        /// </summary>
        /// <param name="object"></param>
        /// <returns></returns>
        protected virtual Descriptor DeserializeDescriptor(JObject @object)
        {
            Guid Parse(string x) => Guid.Parse(x);
            var properties = @object.Properties().ToDictionary(x => x.Name);
            var descriptor = Descriptor.Create();
            // Yes `.Value.Value´, remember we are working with a Dictionary of JToken values here.
            descriptor.SourceFilePath = properties[nameof(descriptor.SourceFilePath)].Value.Value<string>();
            if (properties[nameof(descriptor.GeneratedAssetKeys)].Value is JArray array)
            {
                array.Select(x => Parse(x.Value<string>())).ToList().ForEach(descriptor.GeneratedAssetKeys.Add);
            }

            return descriptor;
        }

        /// <summary>
        /// Returns the Deserialized <see cref="Descriptor"/> <paramref name="objects"/>.
        /// </summary>
        /// <param name="objects"></param>
        /// <returns></returns>
        protected virtual IEnumerable<Descriptor> DeserializeDescriptors(IEnumerable<JObject> objects)
            => objects.Select(DeserializeDescriptor);

        /// <summary>
        /// Deserializes the <paramref name="object"/> into the <paramref name="registry"/>.
        /// </summary>
        /// <param name="object"></param>
        /// <param name="registry"></param>
        /// <returns></returns>
        public virtual TRegistry DeserializeRegistry(JObject @object, TRegistry registry)
        {
            registry.Items.Clear();
            var properties = @object.Properties().ToDictionary(x => x.Name);
            // Ditto working with Dictionary `.Value.Value´.
            registry.OutputDirectory = properties[nameof(registry.OutputDirectory)].Value.Value<string>();
            if (properties[nameof(registry.Items)].Value is JArray array)
            {
                registry.Items = DeserializeDescriptors(array.OfType<JObject>()).ToList();
            }

            return registry;
        }

        /// <inheritdoc />
        public override TRegistry ReadJson(JsonReader reader, Type objectType, TRegistry existingValue
            , bool hasExistingValue, JsonSerializer serializer)
            => DeserializeRegistry(JObject.Load(reader), hasExistingValue ? existingValue : new TRegistry());
    }
}
