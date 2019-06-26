using System;
using System.Collections.Generic;
using System.Linq;

namespace Code.Generation.Roslyn
{
    using static String;

    internal static class VariableListExtensionMethods
    {
        private static bool IsNotNullOrWhiteSpace(string s) => !IsNullOrWhiteSpace(s?.Trim());

        public static IEnumerable<string> Sanitize(this IEnumerable<string> inputs) => inputs.Where(IsNotNullOrWhiteSpace).Select(x => x.Trim());
    }
}
