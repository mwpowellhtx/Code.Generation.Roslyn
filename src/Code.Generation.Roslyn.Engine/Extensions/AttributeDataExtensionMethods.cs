using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace Code.Generation.Roslyn
{
    using Microsoft.CodeAnalysis;
    using Validation;
    using static String;

    internal static class AttributeDataExtensionMethods
    {
        /// <summary>
        /// &apos;,&apos;
        /// </summary>
        private const char Comma = ',';

        /// <summary>
        /// &apos;.&apos;
        /// </summary>
        private const char Dot = '.';

        private class GeneratorTuple : Tuple<Type, AttributeData>
        {
            /// <summary>
            /// Creates a new <see cref="GeneratorTuple"/> instance.
            /// </summary>
            /// <param name="generatorType"></param>
            /// <param name="datum"></param>
            /// <returns></returns>
            internal static GeneratorTuple Create(Type generatorType, AttributeData datum)
                => new GeneratorTuple(generatorType, datum);

            internal Type GeneratorType => Item1;

            private AttributeData Datum => Item2;

            /// <summary>
            /// Private Constructor.
            /// </summary>
            /// <param name="generatorType"></param>
            /// <param name="datum"></param>
            /// <inheritdoc />
            private GeneratorTuple(Type generatorType, AttributeData datum)
                : base(generatorType, datum)
            {
            }

            /// <summary>
            /// Allows for Tuple Deconstruction.
            /// </summary>
            /// <param name="generatorType"></param>
            /// <param name="datum"></param>
            internal void Deconstruct(out Type generatorType, out AttributeData datum)
            {
                generatorType = GeneratorType;
                datum = Datum;
            }
        }

        public static IEnumerable<TGenerator> LoadCodeGenerators<TGenerator>(this ImmutableArray<AttributeData> attributeData, LoadAssemblyCallback loader)
            where TGenerator : class, ICodeGenerator
        {
            Requires.NotNull(loader, nameof(loader));

            TGenerator CreateCodeGenerator(GeneratorTuple tuple)
            {
                var (generatorType, datum) = tuple;
                var generator = Activator.CreateInstance(generatorType, datum);
                Requires.NotNull(generator, nameof(generator));
                // TODO: TBD: might be better if .Is<T> actually returned T... i.e. could simple `return Assumes.Is<ICodeGenerator>(generator);´
                Assumes.Is<TGenerator>(generator);
                return (TGenerator) generator;
            }

            return attributeData
                .Select(x => GeneratorTuple.Create(x.AttributeClass.GetCodeGeneratorTypeForAttribute(loader), x))
                .Where(tuple => tuple.GeneratorType != null).Select(CreateCodeGenerator);
        }

        //private static Type GetCodeGeneratorTypeForAttribute(this INamedTypeSymbol attributeType, AttributeAssemblyLoaderCallback loader)
        private static Type GetCodeGeneratorTypeForAttribute<TSymbol>(this TSymbol attributeType, LoadAssemblyCallback loader)
            where TSymbol : ISymbol
        {
            Requires.NotNull(loader, nameof(loader));

            Type LoadGeneratorTypeFromAssembly(string fullTypeName, string assemblyName)
            {
                var generatorType = assemblyName != null
                    ? loader(new AssemblyName(assemblyName))?.GetType(fullTypeName)
                    : null;

                if (generatorType == null)
                {
                    Verify.FailOperation($"Unable to find code generator `{fullTypeName}´ in `{assemblyName}´.");
                }

                return generatorType;
            }

            // ReSharper disable once InvertIf
            if (attributeType != null)
            {
                var codeGenerationAttributeType = typeof(CodeGenerationAttributeAttribute);

                foreach (var generatorCandidateAttribute in attributeType.GetAttributes()
                    .Where(x => x.AttributeClass.Name == codeGenerationAttributeType.Name))
                {
                    var firstArg = generatorCandidateAttribute.ConstructorArguments.Single();

                    switch (firstArg.Value)
                    {
                        case INamedTypeSymbol typeOfValue:

                            // This was a typeof(T) expression
                            return LoadGeneratorTypeFromAssembly(
                                typeOfValue.GetFullTypeName()
                                , typeOfValue.ContainingAssembly.Name);

                        case string typeName:

                            // This string is the full name of the type, which MAY be assembly-qualified.
                            var commaIndex = typeName.IndexOf(Comma);
                            var isAssemblyQualified = commaIndex >= 0;

                            if (isAssemblyQualified)
                            {
                                return LoadGeneratorTypeFromAssembly(
                                    typeName.Substring(0, commaIndex)
                                    , typeName.Substring(commaIndex + 1).Trim());
                            }
                            else
                            {
                                return LoadGeneratorTypeFromAssembly(
                                    typeName
                                    , generatorCandidateAttribute.AttributeClass.ContainingAssembly.Name);
                            }
                    }
                }
            }

            // Nothing found, but should short-circuit, i.e. "fail", i.e. throw, before reaching this moment.
            return null;
        }

        private static string GetFullTypeName(this INamedTypeSymbol symbol)
        {
            Requires.NotNull(symbol, nameof(symbol));

            IEnumerable<ISymbol> GetSymbolLineage(ISymbol s)
            {
                if (s == null || IsNullOrEmpty(s.Name))
                {
                    yield break;
                }

                // ReSharper disable once TailRecursiveCall
                foreach (var parentSymbol in GetSymbolLineage(s.ContainingSymbol))
                {
                    yield return parentSymbol;
                }

                yield return s;
            }

            return Join($"{Dot}", GetSymbolLineage(symbol).Select(x => x.Name));
        }
    }
}
