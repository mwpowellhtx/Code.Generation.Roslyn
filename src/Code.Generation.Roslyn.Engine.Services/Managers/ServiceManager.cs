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
    /// <typeparam name="TDataTransferObject"></typeparam>
    public abstract class ServiceManager<T, TSet, TDataTransferObject> : ServiceManager
        where TSet : class, IPurgingRegistrySet<T>, new()
    {
        private ObjectToDataTransferObjectCallback<TSet, TDataTransferObject> SetToDataTransferObjectConverter { get; }

        private DataTransferObjectToObjectCallback<TDataTransferObject, TSet> DataTransferObjectToSetConverter { get; }

        /// <summary>
        /// Gets the <typeparamref name="TSet"/> that the Service is Managing.
        /// </summary>
        protected internal TSet RegistrySet { get; private set; }

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
        /// <param name="convertDtoToSetCallback"></param>
        /// <param name="convertSetToDtoCallback"></param>
        protected ServiceManager(string outputDirectory, string registryFileName
            , ObjectToDataTransferObjectCallback<TSet, TDataTransferObject> convertSetToDtoCallback
            , DataTransferObjectToObjectCallback<TDataTransferObject, TSet> convertDtoToSetCallback)
        {
            DataTransferObjectToSetConverter = convertDtoToSetCallback.RequiresNotNull(nameof(convertDtoToSetCallback));
            SetToDataTransferObjectConverter = convertSetToDtoCallback.RequiresNotNull(nameof(convertSetToDtoCallback));

            bool IsNotNullOrEmpty(string s) => !IsNullOrEmpty(s);

            var od = outputDirectory.VerifyOperation(IsNotNullOrEmpty
                , () => FormatVerifyOperationMessage(nameof(outputDirectory))
            );

            var fn = registryFileName.VerifyOperation(IsNotNullOrEmpty
                , () => FormatVerifyOperationMessage(nameof(registryFileName))
            );

            IntermediateOutputDirectory = od;
            RegistrySetPath = Combine(od, fn);

            TryLoad(out _);
        }

        /// <summary>
        /// Tries to Save the <see cref="RegistrySet"/> assuming <see cref="RegistrySetPath"/>.
        /// </summary>
        /// <returns></returns>
        public bool TrySave() => TrySave(RegistrySetPath);

        /// <summary>
        /// Tries to Save the <see cref="RegistrySet"/> given <paramref name="registrySetPath"/>.
        /// </summary>
        /// <param name="registrySetPath"></param>
        /// <returns></returns>
        protected virtual bool TrySave(string registrySetPath)
        {
            try
            {
                var rs = RegistrySet.AssumesTrue(x => Directory.Exists(x.OutputDirectory)
                    , () => $"Output Directory `{RegistrySet.OutputDirectory}´ assumed to exist prior to saving.");

                // We do this because we need to get away from any Set bits that do not require serialization.
                var dto = SetToDataTransferObjectConverter.Invoke(rs);
                var json = JsonConvert.SerializeObject(dto, Formatting.Indented);

                using (var s = File.Open(registrySetPath, FileMode.Create, FileAccess.Write, FileShare.Read))
                {
                    using (var sw = new StreamWriter(s))
                    {
                        sw.Write(json);
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Tries to Load the <paramref name="registrySet"/> assuming
        /// <see cref="RegistrySetPath"/>.
        /// </summary>
        /// <param name="registrySet"></param>
        /// <returns></returns>
        public bool TryLoad(out TSet registrySet)
        {
            var loaded = TryLoad(RegistrySetPath, out registrySet, DataTransferObjectToSetConverter);
            registrySet.OutputDirectory = IntermediateOutputDirectory;
            RegistrySet = registrySet;
            return loaded;
        }

        /// <summary>
        /// Tries to Load the <see cref="IRegistrySet{T}"/> given the
        /// <paramref name="registrySetPath"/>.
        /// </summary>
        /// <param name="registrySetPath"></param>
        /// <param name="registrySet"></param>
        /// <param name="converter"></param>
        /// <returns></returns>
        internal static bool TryLoad(string registrySetPath, out TSet registrySet
            , DataTransferObjectToObjectCallback<TDataTransferObject, TSet> converter)
        {
            registrySet = null;
            try
            {
                // ReSharper disable once RedundantEmptyObjectOrCollectionInitializer
                registrySet = new TSet { };

                if (File.Exists(registrySetPath))
                {
                    using (var s = File.Open(registrySetPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        using (var sr = new StreamReader(s))
                        {
                            var json = sr.ReadToEnd();
                            var dto = JsonConvert.DeserializeObject<TDataTransferObject>(json);
                             registrySet = converter.RequiresNotNull(nameof(converter)).Invoke(dto);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // TODO: TBD: absorb the Exception?
                registrySet = registrySet ?? new TSet { };
            }
            finally
            {
                registrySet = registrySet ?? new TSet { };
            }

            // Assume for the moment that the Output Directory fell out from the given Path.
            registrySet.OutputDirectory = GetDirectoryName(registrySetPath);

            return registrySet?.Any() == true;
        }
    }
}
