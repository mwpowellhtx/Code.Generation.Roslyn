using System;
using System.Collections.Generic;

namespace Code.Generation.Roslyn
{
    using Integration;
    using Xunit;
    using Xunit.Abstractions;
    using static Constants;
    using static Generators.Integration.ModuleKind;
    using AttributeRenderingOptionDictionary = Dictionary<string, object>;
    using IAttributeRenderingOptionDictionary = IDictionary<string, object>;

    public class DocumentTransformationTests : DocumentTransformationTestFixtureBase
    {
        private TestCaseFixture Fixture { get; }

        public DocumentTransformationTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            Fixture = new TestCaseFixture();
        }

        // TODO: TBD: for the time being, the options are `simple´, so inline data is manageable
        // TODO: TBD: however, if they get any more complex, we might consider capturing the Options as a proper Xunit class data `test case´...
        [Theory
         , InlineData(true, true)
         , InlineData(true, false)
         , InlineData(false, true)
         , InlineData(false, false)]
        public void Reflexive_CG_with_Outer_Using_Statement_Is_Correct(bool shorthand, bool fullName)
        {
            Fixture.TryExtrapolate(Bar, out string source).AssertTrue();

            var options = GetAttributeRenderingOptions(shorthand, fullName);

            Fixture.TryAddClassAnnotation<ReflexiveCodeGenerationByTypeAttribute>(source, out source, options).AssertTrue();

            if (!fullName)
            {
                Fixture.TryAddOuterTypeNamespaceUsingStatement<ReflexiveCodeGenerationByTypeAttribute>(source, out source).AssertTrue();
            }

            var expectedSource = $@"{Resources.PreambleCommentText}{CarriageReturnLineFeed}{source}";

            ResolveDocumentTransformation(source, expectedSource);
        }

        [Theory
         , InlineData(true, true)
         , InlineData(true, false)
         , InlineData(false, true)
         , InlineData(false, false)]
        public void Reflexive_CG_with_Inner_Using_Statement_Is_Correct(bool shorthand, bool fullName)
        {
            Fixture.TryExtrapolate(Bar, out string source).AssertTrue();

            var options = GetAttributeRenderingOptions(shorthand, fullName);

            Fixture.TryAddClassAnnotation<ReflexiveCodeGenerationByTypeAttribute>(source, out source, options).AssertTrue();

            if (!fullName)
            {
                Fixture.TryAddInnerTypeNamespaceUsingStatement<ReflexiveCodeGenerationByTypeAttribute>(source, out source).AssertTrue();
            }

            var expectedSource = $@"{Resources.PreambleCommentText}{CarriageReturnLineFeed}{source}";

            ResolveDocumentTransformation(source, expectedSource);
        }

        [Theory
         , InlineData(true, true)
         , InlineData(false, true)
         , InlineData(true, false)
         , InlineData(false, false)]
        public void CG_Correctly_Reads_Around_Non_CG_Annotations_Outer_Using_Statements(bool shorthand, bool fullName)
        {
            Fixture.TryExtrapolate(Bar, out string source).AssertTrue();

            var options = GetAttributeRenderingOptions(shorthand, fullName);

            Fixture.TryAddClassAnnotation<ObsoleteAttribute>(source, out source, options).AssertTrue();
            Fixture.TryAddClassAnnotation<ReflexiveCodeGenerationByTypeAttribute>(source, out source, options).AssertTrue();

            if (!fullName)
            {
                Fixture.TryAddOuterTypeNamespaceUsingStatement<ObsoleteAttribute>(source, out source).AssertTrue();
                Fixture.TryAddOuterTypeNamespaceUsingStatement<ReflexiveCodeGenerationByTypeAttribute>(source, out source).AssertTrue();

                // Also normalizing any whitespace that was injected during the dress rehearsal.
                Fixture.TryNormalizeUsingWhitespace(source, out source).AssertTrue();
            }

            var expectedSource = $@"{Resources.PreambleCommentText}{CarriageReturnLineFeed}{source}";

            ResolveDocumentTransformation(source, expectedSource);
        }

        [Theory
         , InlineData(true, true)
         , InlineData(false, true)
         , InlineData(true, false)
         , InlineData(false, false)]
        public void CG_Correctly_Reads_Around_Non_CG_Annotations_Inner_Using_Statements(bool shorthand, bool fullName)
        {
            Fixture.TryExtrapolate(Bar, out string source).AssertTrue();

            var options = GetAttributeRenderingOptions(shorthand, fullName);

            Fixture.TryAddClassAnnotation<ObsoleteAttribute>(source, out source, options).AssertTrue();
            Fixture.TryAddClassAnnotation<ReflexiveCodeGenerationByTypeAttribute>(source, out source, options).AssertTrue();

            if (!fullName)
            {
                Fixture.TryAddInnerTypeNamespaceUsingStatement<ObsoleteAttribute>(source, out source).AssertTrue();
                Fixture.TryAddInnerTypeNamespaceUsingStatement<ReflexiveCodeGenerationByTypeAttribute>(source, out source).AssertTrue();

                // Also normalizing any whitespace that was injected during the dress rehearsal.
                Fixture.TryNormalizeUsingWhitespace(source, out source).AssertTrue();
            }

            var expectedSource = $@"{Resources.PreambleCommentText}{CarriageReturnLineFeed}{source}";

            ResolveDocumentTransformation(source, expectedSource);
        }

        [Theory
         , InlineData(true, true)
         , InlineData(true, false)
         , InlineData(false, true)
         , InlineData(false, false)]
        public void CG_Uses_Custom_Preamble_Correctly(bool shorthand, bool fullName)
        {
            Fixture.TryExtrapolate(Bar, out string source).AssertTrue();

            var options = GetAttributeRenderingOptions(shorthand, fullName);

            Fixture.TryAddClassAnnotation<WithCustomPreambleAttribute>(source, out source, options).AssertTrue();

            if (!fullName)
            {
                Fixture.TryAddOuterTypeNamespaceUsingStatement<WithCustomPreambleAttribute>(source, out source).AssertTrue();
            }

            var expectedSource = $@"{WithCustomPreambleGenerator.CustomPreambleText}{CarriageReturnLineFeed}{source}";

            ResolveDocumentTransformation(source, expectedSource);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !IsDisposed)
            {
                Fixture.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
