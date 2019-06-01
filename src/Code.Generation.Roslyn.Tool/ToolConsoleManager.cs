using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Code.Generation.Roslyn
{
    using Microsoft.CodeAnalysis;
    using NConsole.Options;
    using static String;

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

            Levels = new ErrorLevelCollection
            {
                {1, () => !SourcePathList.Values.Any(), () => "No source files specified."},
                {2, () => IsNullOrEmpty(OutputDirectory.Value), () => "An output directory must be specified."},
                {3, () => ServiceException != null, RenderServiceException},
                DefaultErrorLevel
            };
        }

        public override void Run(out int errorLevel)
        {
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

                return level == DefaultErrorLevel;
            }

            if (TryReportErrorLevel(errorLevel) || VersionSwitch.Enabled)
            {
                return;
            }

            IEnumerable<string> Sanitize(IEnumerable<string> inputs) => inputs.Where(x => !IsNullOrWhiteSpace(x)).Select(x => x.Trim());

            // TODO: TBD: borderline complexity boundary here, could potentially benefit from a DI container...
            AssemblyReferenceServiceManager CreateReferenceService()
                => new AssemblyReferenceServiceManager(OutputDirectory, IntermediateGeneratedRegistryFileName
                    , Sanitize(ReferencePathList).ToArray(), Sanitize(GeneratorSearchPathList).ToArray());

            var referenceService = CreateReferenceService();

            DocumentTransformation CreateDocumentTransformation() => new DocumentTransformation(referenceService);

            var serviceManager = new CompilationServiceManager(OutputDirectory, IntermediateGeneratedRegistryFileName
                , referenceService, CreateDocumentTransformation())
            {
                ProjectDirectory = ProjectDirectory,
                SourcePathsToCompile = Sanitize(SourcePathList).ToArray(),
                //// TODO: TBD: bits that got refactored to AssemblyReferenceService
                //// TODO: TBD: ... or DocumentTransformation ...
                //AssemblyReferencePath = Sanitize(ReferencePathList).ToArray(),
                PreprocessorSymbols = PreprocessorSymbolsList.ToArray()
                //GeneratorAssemblySearchPaths = Sanitize(GeneratorSearchPathList).ToArray(),
                //IntermediateOutputDirectory = OutputDirectory,
                //IntermediateAssembliesRegistryFileName = IntermediateAssembliesRegistryFileName,
                //IntermediateGeneratedRegistryFileName = IntermediateGeneratedRegistryFileName
            };

            var progress = new Progress<Diagnostic>(d => Writer.WriteLine($"{d}"));

            try
            {
                serviceManager.Generate(progress);
            }
            catch (Exception ex)
            {
                TryReportErrorLevel(errorLevel = 3);
                return;
            }

            void ReportGeneratedFiles(Logging.Logger logger, IDictionary<string, string[]> generatedFiles)
            {
                foreach (var g in generatedFiles)
                {
                    logger.Information($"Code generation triggered by `{g.Key}'...");
                    foreach (var h in g.Value)
                    {
                        logger.Information($"Generated `{h}'.");
                    }
                }
            }

            ReportGeneratedFiles(Logging.Logger.Resource, serviceManager.RegistrySet.GeneratedSourceBundles);
        }
    }
}
