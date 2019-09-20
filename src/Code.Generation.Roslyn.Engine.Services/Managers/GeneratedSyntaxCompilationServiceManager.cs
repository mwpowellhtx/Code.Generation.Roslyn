using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Code.Generation.Roslyn
{
    using static Path;

    // TODO: TBD: this would then be used in a CGR "CLI Tool" project, for which the MSBuild tasks callback into
    // TODO: TBD: however, at that point, I wonder what the issue is with simply delivering with an actual MSBuild custom Task itself?
    public abstract class GeneratedSyntaxCompilationServiceManager<TRegistrySet, TJsonConverter>
        : ServiceManager<GeneratedSyntaxTreeDescriptor, TRegistrySet, TJsonConverter>
        where TRegistrySet : GeneratedSyntaxTreeRegistry, new()
        where TJsonConverter : GeneratedSyntaxTreeRegistryJsonConverter<TRegistrySet>
    {
        /// <summary>
        /// Gets or Sets the set of Source Paths to be Compiled.
        /// </summary>
        public IReadOnlyCollection<string> SourcePathsToCompile { get; set; }

        /// <summary>
        /// Gets or Sets the set of Defined Preprocessor Symbols.
        /// </summary>
        public IReadOnlyCollection<string> PreprocessorSymbols { get; set; }

        /// <summary>
        /// Gets or Sets the Project Directory.
        /// Literally, the Directory in which the &quot;.csproj&quot; file exists.
        /// </summary>
        public string ProjectDirectory { get; set; }

        /// <summary>
        /// ERROR_SHARING_VIOLATION (0x800780020)
        /// </summary>
        /// <see cref="!:https://docs.microsoft.com/en-us/windows/desktop/Debug/system-error-codes--0-499-#ERROR_SHARING_VIOLATION"/>
        protected const int HrProcessCannotAccessFile = unchecked((int) 0x80070020);

        /// <summary>
        /// Initializes a new instance of the Service class with default dependency resolution and loading.
        /// </summary>
        /// <param name="outputDirectory"></param>
        /// <param name="registryFileName"></param>
        /// <param name="converterFactory"></param>
        /// <inheritdoc />
        protected GeneratedSyntaxCompilationServiceManager(string outputDirectory, string registryFileName
            , JsonConverterFactoryCallback<TRegistrySet, TJsonConverter> converterFactory)
            : base(outputDirectory, registryFileName, converterFactory)
        {
        }

        /// <summary>
        /// In addition to Saving the
        /// <see cref="ServiceManager{T,TSet,TDataTransferObject}.Registry"/>, we must
        /// also save a Compilation Response file. This will in turn be used to subsequently
        /// build the then-generated source code.
        /// </summary>
        /// <inheritdoc />
        protected override bool TrySave(string registrySetPath)
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            var compilationResponsePath = Combine(GetDirectoryName(registrySetPath)
                , $"{GetFileNameWithoutExtension(registrySetPath)}.rsp");

            void RemoveOldResponseFile()
            {
                if (!File.Exists(compilationResponsePath))
                {
                    return;
                }

                File.Delete(compilationResponsePath);
            }

            bool result;

            try
            {
                if (!base.TrySave(registrySetPath))
                {
                    RemoveOldResponseFile();
                    return false;
                }

                using (var s = File.Open(compilationResponsePath, FileMode.Create, FileAccess.Write, FileShare.Read))
                {
                    using (var sw = new StreamWriter(s))
                    {
                        // Extrapolate a Compilation Response File for purposes of Target Consumption following Code Gen.
                        foreach (var generated in Registry.SelectMany(x => x.GeneratedAssetKeys.Select(
                            y => Combine(Registry.OutputDirectory, y.RenderGeneratedFileName()))))
                        {
                            sw.WriteLine(generated);
                        }
                    }
                }

                result = true;
            }
            catch (Exception ex)
            {
                RemoveOldResponseFile();
                throw;
            }

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            return result;
        }

        /// <summary>
        /// Returns whether ShouldRetry. Updates the <paramref name="retries"/> value
        /// by the <paramref name="delta"/> quantity.
        /// </summary>
        /// <param name="retries"></param>
        /// <param name="delta"></param>
        /// <returns></returns>
        protected static bool ShouldRetry(ref int retries, int delta = -1) => (retries += delta) > 0;
    }
}
