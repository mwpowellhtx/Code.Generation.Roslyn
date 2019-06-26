using System;
using System.Reflection;

namespace Code.Generation.Roslyn
{
    using static BindingFlags;

    internal static class OperationExtensionMethods
    {
        /// <summary>
        /// Invokes the <paramref name="operation"/> existing on the <paramref name="obj"/>.
        /// Given <paramref name="errorLevel"/> as an argument.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="operation"></param>
        /// <param name="obj"></param>
        /// <param name="errorLevel"></param>
        public static void InvokeOperation<T>(this OperationKind operation, T obj, int errorLevel)
            where T : class
        {
            var operationName = $"On{operation}";
            var method = typeof(T).GetMethod(operationName, NonPublic | Instance, Type.DefaultBinder, new[] {typeof(int)}, null);
            method.Invoke(obj, new object[] {errorLevel});
        }
    }
}
