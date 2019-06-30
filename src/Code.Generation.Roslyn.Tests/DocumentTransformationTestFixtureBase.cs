using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Code.Generation.Roslyn
{
    using Xunit;
    using Xunit.Abstractions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;

    public abstract class DocumentTransformationTestFixtureBase
        : CompilationCodeGenerationTestFixtureBase<AdhocWorkspace, AdhocCompilationManager>
    {
        private Guid TransformationId { get; } = Guid.NewGuid();

        private string TransformationName => $"{TransformationId:D}";

        private string ProjectDirectory => TransformationName;

        private string IntermediateAssemblyReferenceRegistryFileName => $"{TransformationName}.a.json";

        protected DocumentTransformationTestFixtureBase(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            // TODO: TBD: could potentially establish a transformation fixture for internal use to help organize these bits...
            if (!Directory.Exists(TransformationName))
            {
                Directory.CreateDirectory(TransformationName);
            }
        }

        protected virtual void ResolveDocumentTransformation(string source, params string[] expectedResults)
            => ResolveDocumentTransformation(source, (IEnumerable<string>) expectedResults);

        // ReSharper disable once RedundantEmptyObjectOrCollectionInitializer
        private static IProgress<Diagnostic> Progress => new Progress<Diagnostic> { }.AssertNotNull();

        private IEnumerable<string> TransformedSources { get; set; } = GetRange<string>().ToArray();

        protected override void CompilationManager_OnEvaluateCompilation(object sender, CompilationDiagnosticEventArgs e)
        {
            base.CompilationManager_OnEvaluateCompilation(sender, e);

            //// TODO: TBD: may in fact make the base abstract...
            //base.CompilationManager_OnEvaluateCompilation(sender, e);

            // TODO: TBD: in and of itself Creating ReferenceService and DocumentTransformation is based on the Tool project...
            // TODO: TBD: perhaps which could be refactored to a better place...
            // TODO: TBD: fluent assertions notwithstanding...

            // TODO: TBD: do we need any ReferencePathList elements for this?
            var referencePath = GetRange<string>();
            // TODO: TBD: ditto GeneratorSearchPathList ... ?
            var generatorSearchPath = GetRange<string>();

            AssemblyReferenceServiceManager CreateReferenceService() => new AssemblyReferenceServiceManager(
                TransformationName, IntermediateAssemblyReferenceRegistryFileName
                , referencePath.ToArray(), generatorSearchPath.ToArray());

            var docs = e.Project.Documents;

            // TODO: TBD: this approach is loosely informed by the original project:
            // https://github.com/AArnott/CodeGeneration.Roslyn/blob/master/src/CodeGeneration.Roslyn.Tests/Helpers/CompilationTestsBase.cs#L75

            // ReSharper disable PossibleMultipleEnumeration
            docs.Any().AssertTrue();
            // TODO: TBD: "1" (or however many... may want to capture "sources" as a property which we can use to verify...)
            docs.Count().AssertEqual(1);

            // TODO: TBD: may better leverage the tree(s) in the actual e.Compilation instead...
            docs.First().TryGetSyntaxTree(out var tree).AssertTrue();
            // ReSharper restore PossibleMultipleEnumeration

            var transformation = new DocumentTransformation(CreateReferenceService().AssertNotNull())
            {
                ProjectDirectory = ProjectDirectory,
                InputDocument = tree.AssertNotNull()
            };

            var compilation = e.Compilation as CSharpCompilation;

            // TODO: TBD: may want to convey task cancellation from other sources than `default´.
            var results = transformation.TransformAsync(compilation, Progress, default).Result;

            TransformedSources = results.Select(x => $"{x.GetText()}").ToArray();
        }

        protected virtual void ResolveDocumentTransformation(string source, IEnumerable<string> expectedResults)
        {
            // ReSharper disable PossibleMultipleEnumeration
            expectedResults.AssertNotNull();

            ReportSourceLines(nameof(source), source);
            ReportSourceLines(nameof(expectedResults), expectedResults.ToArray());

            CompilationManager.Compile(null, source);

            var actualSources = TransformedSources.ToArray();

            // TODO: TBD: unit test progress...
            // TODO: TBD: CG is happening, preamble is happening, trailing CRLF `trivia´ would happen if we enable the flag for it.
            // TODO: TBD: should also furnish an 'expected' with the indicated changes...
            ReportSourceLines(nameof(actualSources), actualSources);

            TransformedSources.AssertEqual(expectedResults);
            // ReSharper restore PossibleMultipleEnumeration
        }

        private void ReportSourceLines(string name, params string[] source)
        {
            var i = 0;

            OutputHelper.WriteLine($"<{name}>");

            foreach (var lines in source.Select(x => x.Replace("\r\n","\n").Split('\n')))
            {
                ++i;

                var j = 0;

                foreach (var x in lines)
                {
                    OutputHelper.WriteLine($"{nameof(source)}[{i}][{++j}]: {x}");
                }
            }

            OutputHelper.WriteLine($"</{name}>");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !IsDisposed)
            {
                // TODO: TBD: may want to archive any results on failure of any kind...
                if (Directory.Exists(TransformationName))
                {
                    Directory.Delete(TransformationName, true);
                }
            }

            base.Dispose(disposing);
        }
    }
}
