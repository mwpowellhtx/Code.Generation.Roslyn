using System;
using System.IO;
using System.Linq;

namespace Code.Generation.Roslyn
{
    using Newtonsoft.Json;
    using Validation;
    using static Messages;
    using static Path;
    using static String;

    /// <summary>
    /// Service Manager base class.
    /// </summary>
    public abstract class ServiceManager
    {
        /// <summary>
        /// Returns the <see cref="VerifyOperationMustBeSetMessage"/> pattern with the
        /// <paramref name="s"/> argument.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        protected static string FormatVerifyOperationMessage(string s) => Format(VerifyOperationMustBeSetMessage, s);
    }

    /// <inheritdoc />
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TSet"></typeparam>
    /// <typeparam name="TJsonConverter"></typeparam>
    public abstract class ServiceManager<T, TSet, TJsonConverter> : ServiceManager
        where TSet : class, IPurgingRegistrySet<T>, new()
        where TJsonConverter : JsonConverter<TSet>
    {
        private JsonConverterFactoryCallback<TSet, TJsonConverter> ConverterFactory { get; }

        private TSet _registry;

        /// <summary>
        /// Gets the <typeparamref name="TSet"/> that the Service is Managing.
        /// </summary>
        public TSet Registry => _registry;

        /// <summary>
        /// Gets the Directory Path that will contain the Generated Source Files.
        /// </summary>
        protected string IntermediateOutputDirectory { get; }

        private string RegistrySetPath { get; }

        /// <summary>
        /// Protected Constructor.
        /// </summary>
        /// <param name="outputDirectory"></param>
        /// <param name="registryFileName"></param>
        /// <param name="converterFactory"></param>
        protected ServiceManager(string outputDirectory, string registryFileName
            , JsonConverterFactoryCallback<TSet, TJsonConverter> converterFactory)
        {
            ConverterFactory = converterFactory.RequiresNotNull(nameof(converterFactory));

            bool IsNotNullOrEmpty(string s) => !IsNullOrEmpty(s);

            var od = outputDirectory.VerifyOperation(IsNotNullOrEmpty
                , () => FormatVerifyOperationMessage(nameof(outputDirectory))
            );

            var fn = registryFileName.VerifyOperation(IsNotNullOrEmpty
                , () => FormatVerifyOperationMessage(nameof(registryFileName))
            );

            IntermediateOutputDirectory = od;
            RegistrySetPath = Combine(od, fn);

            TryLoad(out _registry);
        }

        /// <summary>
        /// Tries to Save the <see cref="Registry"/> assuming <see cref="RegistrySetPath"/>.
        /// </summary>
        /// <returns></returns>
        public bool TrySave() => TrySave(RegistrySetPath);

        /// <summary>
        /// Tries to Save the <see cref="Registry"/> given <paramref name="registrySetPath"/>.
        /// </summary>
        /// <param name="registrySetPath"></param>
        /// <returns></returns>
        protected virtual bool TrySave(string registrySetPath)
        {
            var registry = Registry
                .RequiresNotNull(nameof(Registry))
                .AssumesTrue(x => Directory.Exists(x.OutputDirectory)
                    , () => $"Output Directory `{Registry.OutputDirectory}´ assumed to exist prior to saving.");

            // We do this because we need to get away from any Set bits that do not require serialization.
            var converter = ConverterFactory.Invoke();
            var json = JsonConvert.SerializeObject(registry, Formatting.Indented, converter.RequiresNotNull(nameof(converter)));

            using (var s = File.Open(registrySetPath, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                using (var sw = new StreamWriter(s))
                {
                    sw.Write(json);
                }
            }

            return true;
        }

        /// <summary>
        /// Tries to Load the <paramref name="registry"/> assuming
        /// <see cref="RegistrySetPath"/>.
        /// </summary>
        /// <param name="registry"></param>
        /// <returns></returns>
        public bool TryLoad(out TSet registry)
        {
            var converter = ConverterFactory.Invoke();
            var loaded = TryLoad(RegistrySetPath, out registry, converter.RequiresNotNull(nameof(converter)));
            registry.AssumesNotNull().OutputDirectory = IntermediateOutputDirectory;
            return loaded;
        }

        /// <summary>
        /// Tries to Load the <see cref="IRegistrySet{T}"/> given the
        /// <paramref name="registrySetPath"/>.
        /// </summary>
        /// <param name="registrySetPath"></param>
        /// <param name="registry"></param>
        /// <param name="converter"></param>
        /// <returns></returns>
        internal static bool TryLoad(string registrySetPath, out TSet registry, TJsonConverter converter)
        {
            registry = null;
            try
            {
                if (File.Exists(registrySetPath))
                {
                    using (var s = File.Open(registrySetPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        using (var sr = new StreamReader(s))
                        {
                            var json = sr.ReadToEnd();
                            var rs = JsonConvert.DeserializeObject<TSet>(json, converter.AssumesNotNull());
                            registry = rs.AssumesNotNull();
                        }
                    }
                }
            }
            finally
            {
                // ReSharper disable once RedundantEmptyObjectOrCollectionInitializer
                registry = registry ?? new TSet { };
            }

            return registry.AssumesNotNull().Any();
        }
    }
}
