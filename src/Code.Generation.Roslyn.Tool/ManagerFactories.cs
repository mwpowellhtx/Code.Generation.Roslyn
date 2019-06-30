using System.Collections.Generic;
using System.Linq;

namespace Code.Generation.Roslyn
{
    internal class ManagerFactories
    {
        private IToolConsoleManager ConsoleManager { get; }

        internal ManagerFactories(IToolConsoleManager consoleManager)
        {
            ConsoleManager = consoleManager;
        }

        private string ProjectDirectory => ConsoleManager.ProjectDirectory;

        private string OutputDirectory => ConsoleManager.OutputDirectory;

        private string IntermediateGeneratedRegistryFileName => ConsoleManager.IntermediateGeneratedRegistryFileName;

        private string IntermediateAssembliesRegistryFileName => ConsoleManager.IntermediateAssembliesRegistryFileName;

        private IReadOnlyCollection<string> ReferencePaths => ConsoleManager.ReferencePathList.Sanitize().ToArray();

        private IReadOnlyCollection<string> GeneratorSearchPaths => ConsoleManager.GeneratorSearchPathList.Sanitize().ToArray();

        private IReadOnlyCollection<string> SourcePaths => ConsoleManager.SourcePathList.Sanitize().ToArray();

        private IReadOnlyCollection<string> PreprocessorSymbols => ConsoleManager.PreprocessorSymbolsList.Sanitize().ToArray();

        private AssemblyReferenceServiceManager _referenceService;

        // TODO: TBD: could potentially refactor these to DI-oriented concerns...
        private AssemblyReferenceServiceManager ReferenceService
            => _referenceService
               ?? (_referenceService = new AssemblyReferenceServiceManager(OutputDirectory
                   , IntermediateAssembliesRegistryFileName, ReferencePaths
                   , GeneratorSearchPaths
               ));

        private AssemblyTransformation AssemblyTransformation => new AssemblyTransformation(ReferenceService) {ProjectDirectory = ProjectDirectory};

        private DocumentTransformation Transformation => new DocumentTransformation(ReferenceService) {ProjectDirectory = ProjectDirectory};

        internal CompilationServiceManager CompilationService => new CompilationServiceManager(
            OutputDirectory, IntermediateGeneratedRegistryFileName, Transformation, AssemblyTransformation)
        {
            ProjectDirectory = ProjectDirectory,
            SourcePathsToCompile = SourcePaths,
            PreprocessorSymbols = PreprocessorSymbols
        };
    }
}
