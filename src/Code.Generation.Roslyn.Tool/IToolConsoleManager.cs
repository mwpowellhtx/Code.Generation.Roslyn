namespace Code.Generation.Roslyn
{
    using NConsole.Options;

    internal interface IToolConsoleManager
    {
        Variable<string> ProjectDirectory { get; }

        Variable<string> OutputDirectory { get; }

        Variable<string> IntermediateGeneratedRegistryFileName { get; }

        Variable<string> IntermediateAssembliesRegistryFileName { get; }

        VariableList<string> ReferencePathList { get; }

        VariableList<string> GeneratorSearchPathList { get; }

        VariableList<string> SourcePathList { get; }

        VariableList<string> PreprocessorSymbolsList { get; }
    }
}
