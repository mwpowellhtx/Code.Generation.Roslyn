using System.Collections.Generic;
using System.IO;

namespace Code.Generation.Roslyn
{
    using Microsoft.CodeAnalysis;
    using static Path;
    using static MSBuildCompilationManagerFixture.KnownExtensions;
    using static Microsoft.CodeAnalysis.OutputKind;

    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// Fixtures the <see cref="MSBuildCompilationManager"/> for internal use.
    /// </summary>
    /// <inheritdoc />
    public class MSBuildCompilationManagerFixture : MSBuildCompilationManager
    {
        //// TODO: TBD: options to save intermediate obj/ files and result bin/ files?
        //// TODO: TBD: ideally wanting to load the resulting Assembly itself for inspection...
        //// TODO: TBD: however, not like this, but we do need to ensure that a Project is `trained´ with the Options prior to requesting Compilation...
        //// TODO: TBD: which, to be honest, I'm not sure why that would not be the case to begin with...
        //protected override CompilationOptions CompilationOptions => base.CompilationOptions;

        //protected override ParseOptions ParseOptions => base.ParseOptions;

        // ReSharper disable InconsistentNaming, IdentifierTypo
        internal static class KnownExtensions
        {
            internal const char dot = '.';

            internal const string exe = nameof(exe);

            internal const string netmodule = nameof(netmodule);

            internal const string winmdobj = nameof(winmdobj);

            internal const string dll = nameof(dll);
        }
        // ReSharper restore InconsistentNaming, IdentifierTypo

        /// <summary>
        /// Specifies a mapping to Known Extensions. We intentionally do not specify
        /// DLL. In other words, if look up fails, then assume DLL as the default.
        /// </summary>
        private static IDictionary<OutputKind, string> OutputExtensions => new Dictionary<OutputKind, string>
        {
            {ConsoleApplication, exe},
            {WindowsApplication, exe},
            {WindowsRuntimeApplication, exe},
            {NetModule, netmodule},
            {WindowsRuntimeMetadata, winmdobj},
        };

        protected override void OnEvaluateCompilation(Project project, ICompilationDiagnosticFilter diagnosticFilter)
        {
            string GetProjectOutputFilePath() => string.IsNullOrEmpty(project.OutputFilePath) ? null : project.OutputFilePath;

            // TODO: TBD: I am kind of surprised that the Roslyn API does not do more to help with this issue?
            string GetOutputExtension(OutputKind kind) => OutputExtensions.TryGetValue(kind, out var ext) ? ext : $"{dot}{dll}";

            // TODO: TBD: ditto Compilation... CompilationWithAnalyzers...
            var compilation = diagnosticFilter.GetCompilation<Compilation>();
            // TODO: TBD: possibly involving Compilation.Emit? pdb path? xml path? comprehension of `target framework´?

            string GetDesiredProjectOutputPath()
            {
                var outputDirectory = Combine($"{project.AssemblyName}", "bin");
                if (!Directory.Exists(outputDirectory))
                {
                    Directory.CreateDirectory(outputDirectory);
                }

                var outputFileName = $"{project.AssemblyName}{GetOutputExtension(compilation.Options.OutputKind)}";

                return Combine(outputDirectory, outputFileName);
            }

            // TODO: TBD: may further report Emit diagnostics ...
            var outputPath = GetProjectOutputFilePath() ?? GetDesiredProjectOutputPath();
            var pdbPath = Combine(GetDirectoryName(outputPath), $"{GetFileNameWithoutExtension(outputPath)}.pdb");

            diagnosticFilter.Result = compilation.Emit(outputPath, pdbPath);

            base.OnEvaluateCompilation(project, diagnosticFilter);
        }
    }
}
