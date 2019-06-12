using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Code.Generation.Roslyn.Integration
{
    using Generators.Integration;
    using static Constants;
    using static Enums;
    using IAttributeRenderingOptionDictionary = IDictionary<string, object>;

    // TODO: TBD: refactor resource oriented bits that load from the resource, replace, etc, to this class
    // TODO: TBD: leverage those for the document transformation unit tests...
    public class TestCaseFixture : TestCaseFixtureBase
    {
        public bool TryLoadFromResource(string resourcePath, out string text)
        {
            using (var rs = FixtureType.Assembly.GetManifestResourceStream(resourcePath))
            {
                using (var reader = new StreamReader(rs))
                {
                    text = reader.ReadToEnd();
                }
            }

            return text.Any();
        }

        protected bool TryInfluenceAttributeAnnotation(string s, out string result, AttributeAnnotationCallback callback)
        {
            result = null;

            // The proper response here is a bit nuanced.
            var x = callback(s).Trim();

            // If we were working with files, great thing is `removing´ it from the project is tantamount to simply deleting it.
            return x.Any() && (result = x) != s;
        }

        public virtual bool TryAddClassAnnotation<TAttribute>(string s, out string text, IAttributeRenderingOptionDictionary options = null)
            where TAttribute : Attribute
            => TryInfluenceAttributeAnnotation(s, out text
                   , x => x.Replace(PublicPartialClass
                       , $"{this.RenderAttributeNotation<TAttribute>(options)}{CarriageReturnLineFeed}    {PublicPartialClass}"));

        public virtual bool TryAddOuterTypeNamespaceUsingStatement<T>(string s, out string text)
            => TryInfluenceAttributeAnnotation(s, out text
                   , x => x.Replace(FooNamespace
                       , $"{Using} {typeof(T).Namespace}{SemiColon}{CarriageReturnLineFeed}{CarriageReturnLineFeed}{FooNamespace}"));

        public virtual bool TryAddInnerTypeNamespaceUsingStatement<T>(string s, out string text)
            => TryInfluenceAttributeAnnotation(s, out text
                   , x => x.Replace($"{FooNamespace}{CarriageReturnLineFeed}{OpenCurlyBrace}"
                       , $"{FooNamespace}{CarriageReturnLineFeed}{OpenCurlyBrace}{CarriageReturnLineFeed}    {Using} {typeof(T).Namespace}{SemiColon}{CarriageReturnLineFeed}"));

        /// <summary>
        /// Normalizes each Using Statement Whitespace in terms of trimming extraneous
        /// leading Carriage Returns and Line Feeds. Works for both Outer as well as
        /// Inner Using Statement appearances.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public virtual bool TryNormalizeUsingWhitespace(string s, out string text)
            => TryInfluenceAttributeAnnotation(s, out text,
                   x => x.Replace($"{CarriageReturnLineFeed}{CarriageReturnLineFeed}{Using}"
                       , $"{CarriageReturnLineFeed}{Using}"))
               || TryInfluenceAttributeAnnotation(s, out text,
                   y => y.Replace($"{CarriageReturnLineFeed}{CarriageReturnLineFeed}    {Using}"
                       , $"{CarriageReturnLineFeed}    {Using}"));

        public virtual bool TryExtrapolate(ModuleKind? modules, out IEnumerable<string> texts)
        {
            bool ModulesDoesContain(ModuleKind module) => modules.Contains(module);

            IEnumerable<string> Extrapolate()
            {
                foreach (var resourceName in GetValues<ModuleKind>().Where(ModulesDoesContain).Select(GetFileName))
                {
                    if (TryLoadFromResource(GetBundledResourcePath(resourceName), out var text))
                    {
                        yield return text;
                    }
                }
            }

            return (texts = Extrapolate().ToArray()).Any();
        }

        public virtual bool TryExtrapolate(ModuleKind? modules, out string text)
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse because `text´ must be assigned
            => (text = null) == null
               && TryExtrapolate(modules, out IEnumerable<string> texts)
               && (text = texts.SingleOrDefault())?.Any() == true;
    }
}
