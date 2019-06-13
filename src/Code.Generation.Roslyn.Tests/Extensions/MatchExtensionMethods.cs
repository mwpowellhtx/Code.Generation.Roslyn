using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Code.Generation.Roslyn
{
    using static String;

    internal static class MatchExtensionMethods
    {
        public static bool HasGroup(this Match match, string groupName) => match.Groups.Any(x => x.Name == groupName);

        public static string GetGroupValue(this Match match, string groupName) => match.Groups[groupName]?.Value;

        public static bool TryGetGroupValue(this Match match, string groupName, out string s) => !IsNullOrEmpty(
            s = match.HasGroup(groupName) ? GetGroupValue(match, groupName) : null
        );
    }
}
