using System;
using System.Collections.Generic;
using System.Linq;

namespace Code.Generation.Roslyn
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Descriptor = AssemblyDescriptor;
    using Registry = AssemblyDescriptorRegistry;

    // TODO: TBD: ditto whether we assume Validation dependencies... Fluently Validate...
    public partial class AssemblyDescriptorRegistryJsonConverter
    {
        /// <summary>
        /// Deserializes the <paramref name="object"/> to <see cref="Descriptor"/>.
        /// </summary>
        /// <param name="object"></param>
        /// <returns></returns>
        protected virtual Descriptor DeserializeDescriptor(JObject @object)
        {
            var properties = @object.Properties().ToDictionary(x => x.Name);
            // Yes, `.Value.Value´, remember, we are dealing with a Dictionary of JToken values.
            var assemblyPath = properties[nameof(Descriptor.AssemblyPath)].Value.Value<string>();
            var descriptor = Descriptor.Create(assemblyPath);
            return descriptor;
        }

        /// <summary>
        /// Deserializes the <paramref name="array"/> to <see cref="IEnumerable{T}"/>
        /// of <see cref="Descriptor"/>.
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        protected virtual IEnumerable<Descriptor> DeserializeDescriptors(JArray array)
            => array.OfType<JObject>().Select(DeserializeDescriptor);

        /// <summary>
        /// Deserializes the <paramref name="object"/> into <paramref name="registry"/>.
        /// </summary>
        /// <param name="object"></param>
        /// <param name="registry"></param>
        /// <returns></returns>
        protected virtual Registry DeserializeRegistry(JObject @object, Registry registry)
        {
            registry.Items.Clear();
            var properties = @object.Properties().ToDictionary(x => x.Name);
            // Ditto Dictionary of JToken values.
            registry.OutputDirectory = properties[nameof(registry.OutputDirectory)].Value.Value<string>();
            if (properties[nameof(registry.OutputDirectory)].Value is JArray array)
            {
                registry.Items = DeserializeDescriptors(array).ToList();
            }

            return registry;
        }

        /// <inheritdoc />
        public override Registry ReadJson(JsonReader reader, Type objectType, Registry existingValue
            , bool hasExistingValue, JsonSerializer serializer)
            => DeserializeRegistry(JObject.Load(reader), hasExistingValue ? existingValue : new Registry());
    }
}
