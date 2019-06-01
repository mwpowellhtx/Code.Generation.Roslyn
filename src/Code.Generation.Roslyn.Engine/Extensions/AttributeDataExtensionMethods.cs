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

        public static IEnumerable<ICodeGenerator> FindCodeGenerators(
            this ImmutableArray<AttributeData> attributeData
            , AttributeAssemblyLoaderCallback loader
        )
        {
            Requires.NotNull(loader, nameof(loader));

            foreach (var generatorType in attributeData.Select(x => x.AttributeClass
                .GetCodeGeneratorTypeForAttribute(loader)).Where(x => x != null))
            {
                var generatorObj = Activator.CreateInstance(generatorType, attributeData);
                Requires.NotNull(generatorObj, nameof(generatorObj));
                // TODO: TBD: might be better if .Is<T> actually returned T...
                Assumes.Is<ICodeGenerator>(generatorObj);
                yield return (ICodeGenerator) generatorObj;
            }
        }

        private static Type GetCodeGeneratorTypeForAttribute<TSymbol>(
            this TSymbol attributeType, AttributeAssemblyLoaderCallback loader)
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
                    Verify.FailOperation($"Unable to find code generator `{fullTypeName}' in `{assemblyName}'.");
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
