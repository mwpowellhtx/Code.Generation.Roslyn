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
    using static OperationKind;

    /// <summary>
    /// The Tool Console Manager.
    /// </summary>
    /// <see cref="OptionSetConsoleManager"/>
    /// <inheritdoc cref="IToolConsoleManager"/>
    internal class ToolConsoleManager : OptionSetConsoleManager, IToolConsoleManager
    {
        private Switch VersionSwitch { get; }

        // TODO: TBD: and if this does not work, then fall back on Switch approach...
        private Variable<OperationKind?> Operation { get; }

        /// <summary>
        /// Gets the <see cref="Variable{T}.Value"/> from <see cref="Operation"/>.
        /// Default is <see cref="Generate"/>.
        /// </summary>
        /// <returns></returns>
        private OperationKind PrivateOperation => ((OperationKind?) Operation) ?? Generate;

        //private Switch GenerateSwitch { get; }
        //private Switch CleanSwitch { get; }

        // TODO: TBD: depend on the Nerdly stuff? for ThisAssembly "code gen" ?
        // TODO: TBD: ways to expose "other versions"? i.e. assembly version?
        private void OnVersion()
        {
            var version = $"{this.GetAssemblyVersion()}";
            var informational = $"{this.GetAssemblyInformationalVersion()}";
            Writer.WriteLine(informational == version ? $"{version}" : $"{version} ({informational})");
        }

        public VariableList<string> ReferencePathList { get; }

        public VariableList<string> PreprocessorSymbolsList { get; }

        public VariableList<string> GeneratorSearchPathList { get; }

        public Variable<string> OutputDirectory { get; }

        public Variable<string> ProjectDirectory { get; }

        public Variable<string> IntermediateAssembliesRegistryFileName { get; }

        public Variable<string> IntermediateGeneratedRegistryFileName { get; }

        public VariableList<string> SourcePathList { get; }

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
            PrivateFactories = new ManagerFactories(this);

            VersionSwitch = Options.AddSwitch("version", OnVersion, "Shows the version of the tool.");

            // TODO: TBD: do we need to do a $"nameof(Operation)" ? or would $"{Operation}" be sufficient?
            Operation = Options.AddVariable<OperationKind?>("operation".MaySpecify()
                , $"[ {nameof(Clean)} | {nameof(Generate)} ]"
                  + $", specify the operation the tool should perform. Default Operation is {nameof(Generate)}.");

            /* While, technically, we `MustSpecify´ Project, Output, and so on, only when the
             * Tool Operation is Generate. However, as an OptionSet, we only `MaySpecify´ the
             * options. We will further vet the Error Levels independently further in. */

            ProjectDirectory = Options.AddVariable<string>("p|project".MaySpecify(), "Project directory absolute path.");
            OutputDirectory = Options.AddVariable<string>("o|output".MaySpecify(), "Generated source files output directory.");
            IntermediateAssembliesRegistryFileName = Options.AddVariable<string>("a|assemblies".MaySpecify(), "JSON formatted intermediate assemblies registry file name.");
            IntermediateGeneratedRegistryFileName = Options.AddVariable<string>("g|generated".MaySpecify(), "JSON formatted intermediate generated registry file name.");

            SourcePathList = Options.AddVariableList<string>("src|source".MaySpecify(), "Source paths included during compilation.");
            ReferencePathList = Options.AddVariableList<string>("r|reference".MaySpecify(), "Paths to assemblies being referenced.");
            PreprocessorSymbolsList = Options.AddVariableList<string>("d|define".MaySpecify(), "Paths to preprocessor symbols.");
            GeneratorSearchPathList = Options.AddVariableList<string>("s|search".MaySpecify(), "Paths to folders that may contain generator assemblies.");

            // Which we should be able to unit test this as well, given our approach.
            ResponseFile = Options.AddVariable<string>("response".MaySpecify(), "Processes argument input from a new line delimited response file.");

            Levels = new ErrorLevelCollection
            {
                // TODO: TBD: and to be fair, we may consider an NConsole version that allows for null message factory...
                // No message should ever be relayed, this should allow --version to short-circuit the rest.
                {DefaultErrorLevel, () => VersionSwitch, () => Empty},
                // TODO: TBD: might do the same for a Help `--help´ or `--?´ option as for Version `--version´...
                {1, () => PrivateOperation == Generate && !SourcePathList.Values.Any(), () => NoSourceFilesSpecified},
                {2, () => IsNullOrEmpty(OutputDirectory.Value), () => OutputDirectoryMustBeSpecified},
                {3, () => ServiceException != null, RenderServiceException},
                DefaultErrorLevel
            };
        }

        /// <summary>
        /// Handles the Service creation aspects for the Manager.
        /// </summary>
        private ManagerFactories PrivateFactories { get; }

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

            bool IsNotNullOrEmpty(string s) => !IsNullOrEmpty((s ?? "").Trim());

            // ReSharper disable once InvertIf
            if (File.Exists(responseFilePath))
            {
                // Allowing for blank lines, leading or trailing whitespace, etc.
                var args = File.ReadAllLines(responseFilePath).Where(IsNotNullOrEmpty).ToArray();
                unparsed = Options.Parse(args);
            }

            return unparsed?.Any() == false;
        }

        // TODO: TBD: refactor this type of functionality to base class, i.e. ReportErrorLevel...
        private bool TryReportErrorLevel(int level)
        {
            // TODO: TBD: perhaps an indexer would be great as well...
            var descriptor = Levels.FirstOrDefault(x => x.ErrorLevel == level);
            // TODO: TBD: perhaps, CanReport property?

            // ReSharper disable once InvertIf
            if (descriptor != null && !IsNullOrEmpty(descriptor.Description))
            {
                switch (level)
                {
                    case Logger.CriticalLevel:
                    case Logger.ErrorLevel:
                        ErrorWriter.WriteLine(descriptor.Description);
                        break;

                    default:
                        Writer.WriteLine(descriptor.Description);
                        break;
                }
            }

            // Error Level will have been Reported.
            return level != DefaultErrorLevel;
        }

        // ReSharper disable UnusedMember.Local, UnusedParameter.Local
        /// <summary>
        /// Callback occurs On <see cref="Generate"/> <see cref="Operation"/>.
        /// </summary>
        /// <param name="_">Receives an Error Level. However, for all intents and purposes, we are
        /// here because any error preconditions apart from <see cref="Logger.CriticalLevel"/> on
        /// <see cref="Exception"/> will have been fully vetted.</param>
        /// <see cref="OperationKind"/>
        /// <see cref="Operation"/>
        /// <see cref="Generate"/>
        private void OnGenerate(int _)
        {
            var compilationService = PrivateFactories.CompilationService;

            var progress = new Progress<Diagnostic>(d => Writer.WriteLine($"{d}"));

            try
            {
                compilationService.Generate(progress);
            }
            catch (Exception ex)
            {
                ServiceException = ex;
                TryReportErrorLevel(Logger.CriticalLevel);
                return;
            }

            void ReportGeneratedFiles(Logger logger, IDictionary<string, string[]> generatedFiles)
            {
                foreach (var (key, value) in generatedFiles)
                {
                    logger.Information($"Code generation triggered by `{key}'...");
                    foreach (var h in value)
                    {
                        logger.Information($"Generated `{h}´.");
                    }
                }
            }

            ReportGeneratedFiles(Logger.Resource, compilationService.RegistrySet.GeneratedSourceBundles);
        }

        /// <summary>
        /// Callback occurs On <see cref="Clean"/> <see cref="Operation"/>.
        /// </summary>
        /// <param name="_">Receives an Error Level. However, for all intents and purposes, we are
        /// here because any error preconditions apart from <see cref="Logger.CriticalLevel"/> on
        /// <see cref="Exception"/> will have been fully vetted.</param>
        /// <see cref="OperationKind"/>
        /// <see cref="Operation"/>
        /// <see cref="Clean"/>
        [Obsolete("Ditto targets Clean work, could potentially drop this one after all...")]
        private void OnClean(int _)
        {
            try
            {
                using (new CleanServiceManager(OutputDirectory
                    , IntermediateGeneratedRegistryFileName
                    , IntermediateAssembliesRegistryFileName))
                {
                }
            }
            catch (Exception ex)
            {
                ServiceException = ex;
                TryReportErrorLevel(Logger.CriticalLevel);
            }
        }
        // ReSharper restore UnusedMember.Local, UnusedParameter.Local

        public override void Run(out int errorLevel)
        {
            /* We are here because we Parsed. We could potentially allow for Parse extensibility,
             * but this will suffice as a workaround for the time being. We do this here because
             * we want to have evaluated the Arguments, regardless of their sourcing, in front of
             * evaluating any error levels. */

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

            // Version trumps Reporting any Error Levels.
            if (VersionSwitch.Enabled || TryReportErrorLevel(errorLevel))
            {
                return;
            }

            /* Kind of a roundabout way of doing it, except to underscore the control flow.
             * The focus is on Invoking the Operation. Which, this should also be demonstrable
             * regardless of the caller context. */
            PrivateOperation.InvokeOperation(this, errorLevel);
        }
    }
}
