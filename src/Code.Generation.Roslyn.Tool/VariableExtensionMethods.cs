using System;

namespace Code.Generation.Roslyn
{
    using NConsole.Options;
    using static String;

    internal static class VariableExtensionMethods
    {
        /// <summary>
        /// Returns whether <paramref name="variable"/> Has the <typeparamref name="T"/> Value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="variable"></param>
        /// <param name="having"></param>
        /// <returns></returns>
        public static bool HasValue<T>(this Variable<T> variable, Func<T, bool> having) => having(variable);

        private static bool IsNotNullOrEmpty(string s) => !IsNullOrEmpty(s);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="variable"></param>
        /// <returns></returns>
        public static bool HasValue(this Variable<string> variable) => variable.HasValue(IsNotNullOrEmpty);
    }
}
