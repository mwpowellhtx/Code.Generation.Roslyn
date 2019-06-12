using System;

namespace Code.Generation.Roslyn.Integration
{
    using static String;

    // TODO: TBD: there are a several other Using Statement variants that might be interesting to leverage...
    // TODO: TBD: but we will leave this alone for the time being and add them if/when necessary...
    /// <summary>
    /// Provides a succinct set of Using Statement oriented extension methods.
    /// </summary>
    internal static class UsingStatementExtensionMethods
    {
        // ReSharper disable once InconsistentNaming
        /// <summary>
        /// The &quot;using&quot; keyword.
        /// </summary>
        private const string @using = nameof(@using);

        /// <summary>
        /// Gets the SemiColon, literally &apos;;&apos;.
        /// </summary>
        private const char SemiColon = ';';

        /// <summary>
        /// Gets the Colon, literally &apos;:&apos;.
        /// </summary>
        private const char Colon = ':';

        /// <summary>
        /// Literally, the Double-Colon, &quot;::&quot;.
        /// </summary>
        /// <see cref="Colon"/>
        private static string DoubleColon => $"{Colon}{Colon}";

        /// <summary>
        /// Renders the <paramref name="global"/> Prefix.
        /// </summary>
        /// <param name="global"></param>
        /// <returns></returns>
        private static string RenderGlobalPrefix(bool global) => global ? $"{nameof(global)}{DoubleColon}" : Empty;

        /// <summary>
        /// Renders the <paramref name="path"/> in terms of a <see cref="@using"/> Statement.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string RenderUsingStatement(this string path, bool global = false) => $"{@using} {RenderGlobalPrefix(global)}{path}{SemiColon}";

        /// <summary>
        /// Renders the <see cref="@using"/> Statement for the <typeparamref name="T"/>
        /// <see cref="Type.Namespace"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="global"></param>
        /// <returns></returns>
        public static string RenderUsingTypeNameSpace<T>(this object anchor, bool global = false) => typeof(T).Namespace.RenderUsingStatement(global);
    }
}
