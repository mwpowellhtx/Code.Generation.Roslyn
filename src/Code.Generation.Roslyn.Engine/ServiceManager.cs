using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Code.Generation.Roslyn
{
    using Newtonsoft.Json;
    using Validation;
    using static Path;
    using static Resources;
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
    public abstract class ServiceManager<T, TSet> : ServiceManager
        where TSet : class, IRegistrySet<T>, new()
    {
        /// <summary>
        /// Gets the <typeparamref name="TSet"/> that the Service is Managing.
        /// </summary>
        internal TSet RegistrySet { get; private set; }

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
        protected ServiceManager(string outputDirectory, string registryFileName)
        {
            Verify.Operation(outputDirectory != null, FormatVerifyOperationMessage(nameof(outputDirectory)));
            Verify.Operation(registryFileName != null, FormatVerifyOperationMessage(nameof(registryFileName)));

            IntermediateOutputDirectory = outputDirectory;

            // ReSharper disable AssignNullToNotNullAttribute
            RegistrySetPath = Combine(outputDirectory, registryFileName);
            // ReSharper restore AssignNullToNotNullAttribute
        }

        /// <summary>
        /// Tries to Save the <see cref="RegistrySet"/> assuming <see cref="RegistrySetPath"/>.
        /// </summary>
        /// <returns></returns>
        internal bool TrySave() => TrySave(RegistrySetPath);

        /// <summary>
        /// Tries to Save the <see cref="RegistrySet"/> given <paramref name="registrySetPath"/>.
        /// </summary>
        /// <param name="registrySetPath"></param>
        /// <returns></returns>
        protected virtual bool TrySave(string registrySetPath)
        {
            try
            {
                RegistrySet.EnsureOutputDirectoryExists();

                var json = JsonConvert.SerializeObject(RegistrySet.ToList(), Formatting.Indented);

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
        internal bool TryLoad(out TSet registrySet)
        {
            var loaded = TryLoad(RegistrySetPath, out registrySet);
            RegistrySet = registrySet;
            return loaded;
        }

        /// <summary>
        /// Tries to Load the <see cref="IRegistrySet{T}"/> given the
        /// <paramref name="registrySetPath"/>.
        /// </summary>
        /// <param name="registrySetPath"></param>
        /// <param name="registrySet"></param>
        /// <returns></returns>
        internal static bool TryLoad(string registrySetPath, out TSet registrySet)
        {
            // ReSharper disable RedundantEmptyObjectOrCollectionInitializer
            registrySet = null;
            try
            {
                registrySet = new TSet { };
                var set = registrySet;

                // We will assume for the time being that a Set.Add at this level will succeed.
                void Add(T item) => set.Add(item);
                if (File.Exists(registrySetPath))
                {
                    using (var s = File.Open(registrySetPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        using (var sr = new StreamReader(s))
                        {
                            var json = sr.ReadToEnd();
                            JsonConvert.DeserializeObject<List<T>>(json)?.ForEach(Add);
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
