using System;
using System.Reflection;

namespace Code.Generation.Roslyn
{
    using static BindingFlags;

    internal static class OperationExtensionMethods
    {
        /// <summary>
        /// &quot;On&quot;
        /// </summary>
        private const string On = nameof(On);

        /// <summary>
        /// Invokes the <paramref name="operation"/> existing on the <paramref name="obj"/>.
        /// Given <paramref name="errorLevel"/> as an argument.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="operation"></param>
        /// <param name="obj"></param>
        /// <param name="errorLevel"></param>
        public static void InvokeOperation<T>(this OperationKind operation, T obj, int errorLevel, string prefix = On)
            where T : class
        {
            var operationName = $"{prefix}{operation}";
            var method = typeof(T).GetMethod(operationName, NonPublic | Instance, Type.DefaultBinder, new[] {typeof(int)}, null);
            method.Invoke(obj, new object[] {errorLevel});
        }
    }
}
