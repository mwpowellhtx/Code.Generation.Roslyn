using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Code.Generation.Roslyn
{
    using Microsoft.CodeAnalysis.Text;
    using Validation;
    using static CSharpSyntaxTree;
    using static Path;
    using static Resources;
    using static Task;
    using static TimeSpan;
    using static OutputKind;

    // TODO: TBD: this would then be used in a CGR "CLI Tool" project, for which the MSBuild tasks callback into
    // TODO: TBD: however, at that point, I wonder what the issue is with simply delivering with an actual MSBuild custom Task itself?
    public class CompilationServiceManager : ServiceManager<GeneratedSyntaxTreeDescriptor, GeneratedSyntaxTreeRegistry>
    {
        private AssemblyReferenceServiceManager ReferenceService { get; }

        private DocumentTransformation Transformation { get; }

        /// <summary>
        /// ERROR_SHARING_VIOLATION (0x800780020)
        /// </summary>
        /// <see cref="!:https://docs.microsoft.com/en-us/windows/desktop/Debug/system-error-codes--0-499-#ERROR_SHARING_VIOLATION"/>
        private const int HrProcessCannotAccessFile = unchecked((int) 0x80070020);

        /// <summary>
        /// Gets or Sets the set of Source Paths to be Compiled.
        /// </summary>
        public IReadOnlyList<string> SourcePathsToCompile { get; set; }

        /// <summary>
        /// Gets or Sets the set of Defined Preprocessor Symbols.
        /// </summary>
        public IEnumerable<string> PreprocessorSymbols { get; set; }

        /// <summary>
        /// Gets or Sets the Project Directory.
        /// Literally, the Directory in which the &quot;.csproj&quot; file exists.
        /// </summary>
        public string ProjectDirectory { get; set; }

        /// <summary>
        /// Initializes a new instance of the Service class with default dependency resolution and loading.
        /// </summary>
        /// <param name="outputDirectory"></param>
        /// <param name="referenceService"></param>
        /// <param name="transformation"></param>
        /// <inheritdoc />
        public CompilationServiceManager(string outputDirectory, string registryFileName
            , AssemblyReferenceServiceManager referenceService, DocumentTransformation transformation)
            : base(outputDirectory, registryFileName)
        {
            Verify.Operation(referenceService != null, FormatVerifyOperationMessage(nameof(referenceService)));
            Verify.Operation(transformation != null, FormatVerifyOperationMessage(nameof(transformation)));

            ReferenceService = referenceService;
            Transformation = transformation;
        }

        /// <summary>
        /// In addition to Saving the <see cref="ServiceManager{T,TSet}.RegistrySet"/>,
        /// we must also save a Compilation Response file. This will in turn be used to
        /// subsequently build the then-generated source code.
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
                        foreach (var generated in RegistrySet.SelectMany(x => x.GeneratedAssetKeys.Select(
                            y => Combine(RegistrySet.OutputDirectory, $"{y:D}.g.cs"))))
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
        /// Runs the code generation as configured using this instance's properties.
        /// </summary>
        /// <param name="progress">Optional handler of diagnostics provided by code generator.</param>
        /// <param name="cancellationToken">Cancellation token to interrupt async operations.</param>
        public void Generate(IProgress<Diagnostic> progress = null, CancellationToken cancellationToken = default)
        {
            Verify.Operation(SourcePathsToCompile != null, FormatVerifyOperationMessage(nameof(SourcePathsToCompile)));

            var compilation = CreateCompilation(cancellationToken);

            // TODO: TBD: we should have a pretty good idea whether an initial compilation would be successful...
            // TODO: TBD: as to whether any subsequent code generation would be justified...
            // TODO: TBD: and somehow capture/report those errors through caller channels...

            if (ReferenceService.TryLoad(out _))
            {
                ReferenceService.PurgeNotExists();
            }

            // For incremental build, we want to consider the input->output files as well as the assemblies involved in code generation.
            var assembliesLastWritten = ReferenceService.AssembliesLastWrittenTimestamp;

            TryLoad(out _);

            // TODO: TBD: just a list of Exceptions? with comprehension over the InputSyntaxTree (below) ?
            var fileFailures = new List<Exception>();

            /* Purge any generated code that was based on files that
             * have recently been removed or in any way renamed. */
            compilation.SyntaxTrees.Select(x => x.FilePath)
                .Where(x => RegistrySet.All(y => x != y.SourceFilePath)).ToList()
                .ForEach(x => RegistrySet.RemoveWhere(y => y.SourceFilePath == x))
                ;

            /* We cannot reasonably know much about the generated code itself, but we can
             * Purge based on the Input File in comparison with the Last Written Assemblies. */
            RegistrySet.Select(x => x.SourceFilePath).ToArray()
                .Where(x => compilation.SyntaxTrees.Any(y => x == y.FilePath)
                            && File.GetLastWriteTimeUtc(x) > assembliesLastWritten).ToList()
                .ForEach(x => RegistrySet.RemoveWhere(y => y.SourceFilePath == x))
                ;

            // TODO: TBD: need to pre-load the assemblies here ?
            // TODO: TBD: trim/shake the references if they do not exist at this moment ?

            cancellationToken.ThrowIfCancellationRequested();

            bool ShouldRetry(ref int retries, int delta = -1) => (retries += delta) > 0;

            var eligibleSyntaxTrees = compilation.SyntaxTrees.Where(
                x => RegistrySet.All(y => x.FilePath != y.SourceFilePath)).ToArray();

            foreach (var currentSyntaxTree in eligibleSyntaxTrees)
            {
                // Do not stage anything. Instead evaluate each Descriptor based on what was Generated.
                var genDescriptor = GeneratedSyntaxTreeDescriptor.Create(currentSyntaxTree.FilePath);

                var generatedCompilationUnits = Transformation.TransformAsync(compilation, currentSyntaxTree
                    , ProjectDirectory, progress, cancellationToken).Result.ToArray();

                foreach (var generatedCompilationUnit in generatedCompilationUnits)
                {
                    // TODO: TBD: this is the core of the code generated assets...
                    var generatedSyntaxTree = generatedCompilationUnit.SyntaxTree;
                    var genText = generatedSyntaxTree.GetText(cancellationToken);
                    var genId = Guid.NewGuid();

                    var actualRetries = 3;

                    try
                    {
                        /* This is ordinarily auto-created by environmental tooling. However, depending
                         on when our CG tooling `sees´ the event, the paths may not entirely exist yet. */

                        // TODO: TBD: may need the full path?
                        var outputPath = Combine(RegistrySet.EnsureOutputDirectoryExists().OutputDirectory, $"{genId:D}.g.cs");

                        if (File.Exists(outputPath))
                        {
                            File.Delete(outputPath);
                        }

                        using (var s = File.Open(outputPath, FileMode.Create, FileAccess.Write, FileShare.Read))
                        {
                            using (var sw = new StreamWriter(s))
                            {
                                sw.Write(genText);
                            }
                        }

                        genDescriptor.GeneratedAssetKeys.Add(genId);
                    }
                    // ReSharper disable once IdentifierTypo
                    catch (IOException ioex) when (ioex.HResult == HrProcessCannotAccessFile && ShouldRetry(ref actualRetries))
                    {
                        // ReSharper disable once MethodSupportsCancellation
                        Delay(FromMilliseconds(200d)).Wait(cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        ReportError(progress, CodeGenerationErrorCode, currentSyntaxTree, ex);
                        fileFailures.Add(ex);
                        break;
                    }
                }

                RegistrySet.Add(genDescriptor);
            }

            // Throw sooner if there were any file failures.
            if (fileFailures.Count > 0)
            {
                throw new AggregateException(fileFailures);
            }

            // Shake out the Compilation Set one last time.
            RegistrySet.RemoveWhere(x => !x.GeneratedAssetKeys.Any());

            // TODO: TBD: need to revisit these calls...
            // TODO: TBD: somewhere during the course of add references, etc, those should probably be registering with the service...
            ReferenceService.TrySave();

            TrySave();
        }

        private static void ReportError(IProgress<Diagnostic> progress, string id, SyntaxTree inputSyntaxTree, Exception exception)
        {
            Console.Error.WriteLine(ErrorReportConsoleMessageFormat, exception);

            if (progress == null)
            {
                return;
            }

            const string messageFormat = "{0}";

            var descriptor = new DiagnosticDescriptor(id, ErrorReportDiagnosticDescriptorTitle, messageFormat
                , ErrorReportDiagnosticDescriptorCategory, DiagnosticSeverity.Error, true);

            var location = inputSyntaxTree == null
                ? Location.None
                : Location.Create(inputSyntaxTree, TextSpan.FromBounds(0, 0));

            progress.Report(Diagnostic.Create(descriptor, location, exception));
        }

        private static MetadataReference CreateMetadataReferenceFromFile(string path)
            => MetadataReference.CreateFromFile(path);

        private static SourceText FromSourceStream(Stream stream) => SourceText.From(stream);

        private CSharpCompilation CreateCompilation(CancellationToken cancellationToken)
        {
            var compilation = CSharpCompilation.Create(CompilationCreatedAssemblyName)
                .WithOptions(new CSharpCompilationOptions(DynamicallyLinkedLibrary))
                .WithReferences(ReferenceService.ReferencePath.Select(CreateMetadataReferenceFromFile));

            var parseOptions = new CSharpParseOptions(preprocessorSymbols: PreprocessorSymbols);

            foreach (var sourcePath in SourcePathsToCompile)
            {
                using (var s = File.Open(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var srcText = FromSourceStream(s);
                    compilation = compilation.AddSyntaxTrees(
                        ParseText(srcText, parseOptions, sourcePath, cancellationToken)
                    );
                }
            }

            return compilation;
        }
    }
}
