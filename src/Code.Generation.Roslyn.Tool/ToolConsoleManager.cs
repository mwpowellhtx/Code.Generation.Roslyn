using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Code.Generation.Roslyn
{
    using Logging;
    using Microsoft.CodeAnalysis;
    using NConsole.Options;
    using static String;
    using static StringLiterals;

    internal class ToolConsoleManager : OptionSetConsoleManager
    {
        private Switch VersionSwitch { get; }

        // TODO: TBD: depend on the Nerdly stuff? for ThisAssembly "code gen" ?
        // TODO: TBD: ways to expose "other versions"? i.e. assembly version?
        private void OnVersion()
        {
            var version = $"{this.GetAssemblyVersion()}";
            var informational = $"{this.GetAssemblyInformationalVersion()}";
            Writer.WriteLine(informational == version ? $"{version}" : $"{version} ({informational})");
        }

        private VariableList<string> ReferencePathList { get; }

        private VariableList<string> PreprocessorSymbolsList { get; }

        private VariableList<string> GeneratorSearchPathList { get; }

        private Variable<string> OutputDirectory { get; }

        private Variable<string> ProjectDirectory { get; }

        private Variable<string> IntermediateAssembliesRegistryFileName { get; }

        private Variable<string> IntermediateGeneratedRegistryFileName { get; }

        private VariableList<string> SourcePathList { get; }

        /// <summary>
        /// Gets the ResponseFile Variable. A ResponseFile may be provide in lieu
        /// of or in addition to Arguments appearing at the Command Line.
        /// </summary>
        private Variable<string> ResponseFile { get; }

        private Exception ServiceException { get; set; }

        private string RenderServiceException()
        {
            var ex = ServiceException;
            using (var writer = new StringWriter())
            {
                // ReSharper disable once InvertIf
                if (ex != null)
                {
                    writer.WriteLine($"{ex.GetType().FullName}: {ex.Message}");
                    writer.WriteLine($"{ex}");
                }

                return $"{writer}";
            }
        }

        internal ToolConsoleManager(TextWriter writer, TextWriter errorWriter = null)
            : base($"{typeof(ToolConsoleManager).Namespace}.Tool", writer
                , errorWriter: errorWriter)
        {
            VersionSwitch = Options.AddSwitch("version", OnVersion, "Shows the version of the tool.");
            ReferencePathList = Options.AddVariableList<string>("r|reference".MaySpecify(), "Paths to assemblies being referenced.");
            PreprocessorSymbolsList = Options.AddVariableList<string>("d|define", "Paths to preprocessor symbols.");
            GeneratorSearchPathList = Options.AddVariableList<string>("s|search", "Paths to folders that may contain generator assemblies.");
            OutputDirectory = Options.AddVariable<string>("o|output", "Generated source files output directory.");
            ProjectDirectory = Options.AddVariable<string>("p|project", "Project directory absolute path.");
            IntermediateAssembliesRegistryFileName = Options.AddVariable<string>("a|assemblies", "JSON formatted intermediate assemblies registry file name.");
            IntermediateGeneratedRegistryFileName = Options.AddVariable<string>("g|generated", "JSON formatted intermediate generated registry file name.");
            SourcePathList = Options.AddVariableList<string>("src|source", "Source paths included during compilation.");
            // Which we should be able to unit test this as well, given our approach.
            ResponseFile = Options.AddVariable<string>("response", "Processes argument input from a new line delimited response file.");

            Levels = new ErrorLevelCollection
            {
                {1, () => !SourcePathList.Values.Any(), () => NoSourceFilesSpecified},
                {2, () => IsNullOrEmpty(OutputDirectory.Value), () => OutputDirectoryMustBeSpecified},
                {3, () => ServiceException != null, RenderServiceException},
                DefaultErrorLevel
            };
        }

        /// <summary>
        /// Tries to Load the <paramref name="responseFilePath"/> Arguments. Try not to do much
        /// in the way of stupid, like providing ANOTHER Response File. That would be silly.
        /// Otherwise, virtually any Arguments may be specified in this way in addition to
        /// at the Command Line itself.
        /// </summary>
        /// <param name="responseFilePath"></param>
        /// <returns></returns>
        private bool TryLoadResponseFile(string responseFilePath)
        {
            IEnumerable<string> unparsed = null;

            // ReSharper disable once InvertIf
            if (File.Exists(responseFilePath))
            {
                // TODO: TBD: should be fine in and of itself... may want to consider the new line specifier? we will see...
                var args = File.ReadAllLines(responseFilePath);
                unparsed = Options.Parse(args);
            }

            return unparsed?.Any() == false;
        }

        public override void Run(out int errorLevel)
        {
            /* We are here because we Parsed. We could potentially allow for Parse extensibility,
             but this will suffice as a workaround for the time being. We do this here because we
             want to have evaluated the Arguments, regardless of their sourcing, in front of
             evaluating any error levels. */

            // Allowing for a Response File as input instead of the direct Command Line Arguments.
            bool TryEvaluateResponseFile(out int responseLevel)
            {
                responseLevel = DefaultErrorLevel;
                // TODO: TBD: we might otherwise throw on this one...
                return !ResponseFile.HasValue() || TryLoadResponseFile(ResponseFile.Value);
            }

            if (!TryEvaluateResponseFile(out errorLevel))
            {
                return;
            }
            // From this point, we should have arguments parsed, where ever we sourced them.

            base.Run(out errorLevel);

            // TODO: TBD: refactor this type of functionality to base class, i.e. ReportErrorLevel...
            bool TryReportErrorLevel(int level)
            {
                // TODO: TBD: perhaps an indexer would be great as well...
                var descriptor = Levels.FirstOrDefault(x => x.ErrorLevel == level);
                // TODO: TBD: perhaps, CanReport property?
                if (descriptor != null && !IsNullOrEmpty(descriptor.Description))
                {
                    Writer.WriteLine(descriptor.Description);
                }

                // Error Level will have been Reported.
                return level != DefaultErrorLevel;
            }

            // Version trumps Reporting any Error Levels.
            if (VersionSwitch.Enabled || TryReportErrorLevel(errorLevel))
            {
                return;
            }

            IEnumerable<string> Sanitize(IEnumerable<string> inputs) => inputs.Where(
                x => !IsNullOrWhiteSpace(x)).Select(x => x.Trim()
            );

            // TODO: TBD: borderline complexity boundary here, could potentially benefit from a DI container...
            AssemblyReferenceServiceManager CreateReferenceService()
                => new AssemblyReferenceServiceManager(OutputDirectory, IntermediateAssembliesRegistryFileName
                    , Sanitize(ReferencePathList).ToArray(), Sanitize(GeneratorSearchPathList).ToArray());

            var referenceService = CreateReferenceService();

            DocumentTransformation CreateDocumentTransformation() => new DocumentTransformation(referenceService);

            var serviceManager = new CompilationServiceManager(OutputDirectory, IntermediateGeneratedRegistryFileName
                , referenceService, CreateDocumentTransformation())
            {
                ProjectDirectory = ProjectDirectory,
                SourcePathsToCompile = Sanitize(SourcePathList).ToArray(),
                PreprocessorSymbols = PreprocessorSymbolsList.ToArray()
            };

            var progress = new Progress<Diagnostic>(d => Writer.WriteLine($"{d}"));

            try
            {
                serviceManager.Generate(progress);
            }
            catch (Exception ex)
            {
                TryReportErrorLevel(errorLevel = Logger.CriticalLevel);
                return;
            }

            void ReportGeneratedFiles(Logger logger, IDictionary<string, string[]> generatedFiles)
            {
                foreach (var (key, value) in generatedFiles)
                {
                    logger.Information($"Code generation triggered by `{key}'...");
                    foreach (var h in value)
                    {
                        logger.Information($"Generated `{h}'.");
                    }
                }
            }

            ReportGeneratedFiles(Logger.Resource, serviceManager.RegistrySet.GeneratedSourceBundles);
        }
    }
}
