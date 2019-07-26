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
    using static GeneratedSyntaxTreeDescriptor;
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

        private AssemblyTransformation AssemblyTransformation { get; }

        private DocumentTransformation Transformation { get; }

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
        /// <param name="transformation"></param>
        /// <param name="assemblyTransformation"></param>
        /// <inheritdoc />
        public CompilationServiceManager(string outputDirectory, string registryFileName
            , DocumentTransformation transformation, AssemblyTransformation assemblyTransformation)
            : base(outputDirectory, registryFileName)
        {
            Requires.NotNull(transformation, nameof(transformation));
            Requires.NotNull(transformation.ReferenceService, nameof(transformation.ReferenceService));
            Requires.NotNull(assemblyTransformation, nameof(assemblyTransformation));
            Requires.NotNull(assemblyTransformation.ReferenceService, nameof(assemblyTransformation.ReferenceService));
            Verify.Operation(ReferenceEquals(transformation.ReferenceService, assemblyTransformation.ReferenceService), "Reference Service must be the same instance.");

            ReferenceService = transformation.ReferenceService;
            Transformation = transformation;
            AssemblyTransformation = assemblyTransformation;
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
        /// Purges the Compilation Registry given <paramref name="compilation"/>
        /// and <paramref name="assembliesLastWritten"/>.
        /// </summary>
        /// <param name="compilation"></param>
        /// <param name="assembliesLastWritten"></param>
        private void PurgeRegistrySet(CSharpCompilation compilation, DateTime? assembliesLastWritten)
        {
            // TODO: TBD: will likely need to revisit `purge´ over Assembly-level CG...

            // The Current set of Compilation FilePaths...
            var compilationFilePaths = compilation.SyntaxTrees.Select(x => x.FilePath).ToArray();

            {
                // ReSharper disable once ImplicitlyCapturedClosure
                // This is a type-specific capture of the Compilation FilePaths, that lets us more concisely Where...
                bool CompilationFilePathsDoNotContain(string registrySourceFilePath)
                    => !compilationFilePaths.Contains(registrySourceFilePath);

                Requires.NotNull(RegistrySet, $"`{nameof(RegistrySet)}´ instance required.");

                // TODO: TBD: it might be interesting to capture/report in some way the purged counts...
                // TODO: TBD: if for nothing else than metric, static/performance analysis, purposes...
                /* Purge any generated code that was based on files that have recently been
                 * renamed, moved, or removed, in which case(s), we want to re-gen. */
                var purgedFileDeltaCount = RegistrySet.Select(x => x.SourceFilePath)
                        .Where(CompilationFilePathsDoNotContain).ToList()
                        .Sum(x => RegistrySet.PurgeWhere(y => y.SourceFilePath == x))
                    ;
            }

            {
                bool GeneratedAssetDoesNotExist(Guid assetId)
                    => !File.Exists(RegistrySet.MakeRelativeSourcePath(assetId));

                var purgedGeneratedGapsCount = RegistrySet
                        .Where(x => x.GeneratedAssetKeys.Any(GeneratedAssetDoesNotExist)).ToList()
                        .Sum(x => RegistrySet.PurgeWhere(y => ReferenceEquals(y, x)))
                    ;
            }

            /* To this point we have ruled out artifacts which will obviously have changed
             * and which require re-gen. Now we must consider bypassing artifacts which do
             * not require re-gen. Eligibility Purging is less strong than the Generated.
             * In other words, in this case, we want to preserve whatever generated assets
             * might be previously existing, and which subsequently do not require re-gen. */

            // Working from the Existing Registry Set.
            IneligibleSet = new EligibleSyntaxTreeRegistry(RegistrySet);

            /* Because we are working from the previous CG iteration, we cannot know
             * POSITIVELY which additional artifacts might possibly be ELIGIBLE, but
             * rather, we CAN know NEGATIVELY which artifacts ought to be INELIGIBLE.
             * In order to do this, we want to establish a clear DateTime boundary,
             * the earliest possible triggering moment meeting the earliest possible
             * historical generated moment, and we want to re-gen assets meeting this
             * triggering condition. Yes, we do want the EARLIEST possible moments for
             * both sides of the trigger condition. */

            // TODO: TBD: we might want to break out elements of these embedded conditions...
            var ineligiblePurgeCount = IneligibleSet
                .Where(x => compilationFilePaths.Any(y => y == x.SourceFilePath)
                            && x.GeneratedAssetKeys
                                .Any(g => IneligibleSet.MakeRelativeSourcePath(g).GetLastWriteTimeUtc()
                                          < x.SourceFilePath.GetLastWriteTimeUtc().Max(assembliesLastWritten))
                ).ToArray()
                .Sum(x => IneligibleSet.PurgeWhere(y => y.SourceFilePath == x.SourceFilePath));

            /* From this point on, excepting for reconciliation during bookkeeping
             * opportunities, we want to work with the EligibleSet. */
        }
        // ReSharper disable once RedundantEmptyObjectOrCollectionInitializer
        /// <summary>
        /// Gets the set of FileFailures.
        /// </summary>
        private IList<Exception> FileFailures { get; } = new List<Exception> { };

        /// <summary>
        /// Returns whether ShouldRetry. Updates the <paramref name="retries"/> value
        /// by the <paramref name="delta"/> quantity.
        /// </summary>
        /// <param name="retries"></param>
        /// <param name="delta"></param>
        /// <returns></returns>
        private static bool ShouldRetry(ref int retries, int delta = -1) => (retries += delta) > 0;

        private delegate void CodeGenerationFacilitationCallback(CSharpCompilation compilation, IProgress<Diagnostic> progress, CancellationToken cancellationToken);

        private delegate TTransformation ConfigureTransformationCallback<TTransformation>(TTransformation transformation)
            where TTransformation : TransformationBase;

        /// <summary>
        /// Transforms the <paramref name="compilation"/> given the other criteria.
        /// </summary>
        /// <typeparam name="TTransformation"></typeparam>
        /// <param name="compilation">A compilation.</param>
        /// <param name="progress">Reporting progress.</param>
        /// <param name="cancellationToken">For asynchronous purposes.</param>
        /// <param name="transformation">A Transformation.</param>
        /// <param name="sourceFilePath">An optional Source File Path.</param>
        /// <param name="configure">Configures the <paramref name="transformation"/>. Should return the same.</param>
        private void TransformCompiledCode<TTransformation>(CSharpCompilation compilation, IProgress<Diagnostic> progress, CancellationToken cancellationToken
            , TTransformation transformation, string sourceFilePath = DefaultSourceFilePath, ConfigureTransformationCallback<TTransformation> configure = null)
            where TTransformation : TransformationBase
        {
            Requires.NotNull(compilation, nameof(compilation));
            Requires.NotNull(progress, nameof(progress));
            Requires.NotNull(transformation, nameof(transformation));

            var descriptor = Create(sourceFilePath);

            var generatedCompilationUnits = (configure?.Invoke(transformation) ?? transformation)
                .TransformAsync(compilation, progress, cancellationToken).Result.ToArray();

            // TODO: TBD: will probably need some sort of OnException handlers for case specific Exception responses...
            foreach (var generatedCompilationUnit in generatedCompilationUnits)
            {
                // TODO: TBD: this is the core of the code generated assets...
                var generatedSyntaxTree = generatedCompilationUnit.SyntaxTree;
                var genText = generatedSyntaxTree.GetText(cancellationToken);
                // TODO: TBD: instead of keeping a loose Id here, could potentially map the CUS's to a Uuid-keyed dictionary.
                var genId = Guid.NewGuid();

                var actualRetries = 3;

                try
                {
                    /* This is ordinarily auto-created by environmental tooling. However, depending
                     * on when our CG tooling `sees´ the event, the paths may not entirely exist yet. */

                    // TODO: TBD: may need the full path?
                    var outputPath = RegistrySet.MakeRelativeSourcePath(genId);

                    // Create/Truncate should be sufficient.
                    using (var s = File.Open(outputPath, FileMode.Create, FileAccess.Write, FileShare.Read))
                    {
                        using (var sw = new StreamWriter(s))
                        {
                            sw.Write(genText);
                        }
                    }

                    descriptor.GeneratedAssetKeys.Add(genId);
                }
                // ReSharper disable once IdentifierTypo
                catch (IOException ioex) when (ioex.HResult == HrProcessCannotAccessFile && ShouldRetry(ref actualRetries))
                {
                    // ReSharper disable once MethodSupportsCancellation
                    Delay(FromMilliseconds(200d)).Wait(cancellationToken);
                }
                catch (Exception ex)
                {
                    ReportError(progress, CodeGenerationErrorCode, ex);
                    FileFailures.Add(ex);
                    break;
                }
            }

            // TODO: TBD: potentially refactor this to more generic methods...
            RegistrySet.Add(descriptor);
        }

        private void FacilitateAssemblyCodeGeneration(CSharpCompilation compilation, IProgress<Diagnostic> progress, CancellationToken cancellationToken)
            => TransformCompiledCode(compilation, progress, cancellationToken, AssemblyTransformation);

        /// <summary>
        /// Gets or Sets the IneligibleSet.
        /// </summary>
        private EligibleSyntaxTreeRegistry IneligibleSet { get; set; }

        private void FacilitateDocumentCodeGeneration(CSharpCompilation compilation, IProgress<Diagnostic> progress, CancellationToken cancellationToken)
        {
            Requires.NotNull(IneligibleSet, nameof(IneligibleSet));

            var eligibleSyntaxTrees = compilation.SyntaxTrees.Where(
                x => IneligibleSet.All(y => x.FilePath != y.SourceFilePath)).ToArray();

            foreach (var currentSyntaxTree in eligibleSyntaxTrees)
            {
                TransformCompiledCode(compilation, progress, cancellationToken, Transformation
                    , currentSyntaxTree.FilePath
                    , x => x.Configure(y => y.InputDocument = currentSyntaxTree)
                );
            }
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

            TryLoad(out _);

            PurgeRegistrySet(compilation, ReferenceService.AssembliesLastWrittenTimestamp);

            // TODO: TBD: need to pre-load the assemblies here ?
            // TODO: TBD: trim/shake the references if they do not exist at this moment ?

            cancellationToken.ThrowIfCancellationRequested();

            foreach (var callback in new CodeGenerationFacilitationCallback[]
            {
                FacilitateAssemblyCodeGeneration,
                FacilitateDocumentCodeGeneration
            })
            {
                callback.Invoke(compilation, progress, cancellationToken);
            }

            // Throw sooner if there were any file failures.
            if (FileFailures.Count > 0)
            {
                throw new AggregateException(FileFailures);
            }

            // Shake out the Compilation Set one last time; Remove is sufficient, no need to Purge anything.
            RegistrySet.RemoveWhere(x => !x.GeneratedAssetKeys.Any());

            // TODO: TBD: need to revisit these calls...
            // TODO: TBD: somewhere during the course of add references, etc, those should probably be registering with the service...
            ReferenceService.TrySave();

            TrySave();
        }

        private static void ReportError(IProgress<Diagnostic> progress, string id, Exception exception)
        {
            Console.Error.WriteLine(ErrorReportConsoleMessageFormat, exception);

            if (progress == null)
            {
                return;
            }

            const string messageFormat = "{0}";

            var descriptor = new DiagnosticDescriptor(id, ErrorReportDiagnosticDescriptorTitle, messageFormat
                , ErrorReportDiagnosticDescriptorCategory, DiagnosticSeverity.Error, true);

            progress.Report(Diagnostic.Create(descriptor, Location.None, exception));
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
