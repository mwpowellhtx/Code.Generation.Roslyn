using System;
using System.Collections.Generic;

namespace Code.Generation.Roslyn.Generators.Integration
{
    internal static class ModuleKindExtensionMethods
    {
        public static bool Contains(this ModuleKind value, ModuleKind mask) => (value & mask) == mask;

        public static bool Contains(this ModuleKind? value, ModuleKind mask) => value.HasValue && (value.Value & mask) == mask;

        public static IEnumerable<ModuleKind> Distinct(this ModuleKind value)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var mask in Enum.GetValues(typeof(ModuleKind)).ToArray<ModuleKind>())
            {
                if (value.Contains(mask))
                {
                    yield return mask;
                }
            }
        }

        public static IEnumerable<ModuleKind> Distinct(this ModuleKind? value)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var mask in Enum.GetValues(typeof(ModuleKind)).ToArray<ModuleKind>())
            {
                if (value.Contains(mask))
                {
                    yield return mask;
                }
            }
        }
    }
}
