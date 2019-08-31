using System;

namespace Code.Generation.Roslyn
{
    using static DescriptorExtensionMethods.Constants;

    /// <summary>
    /// Provides a set of helpful Extension Methods.
    /// </summary>
    public static class DescriptorExtensionMethods
    {
        // ReSharper disable InconsistentNaming
        /// <summary>
        /// Constants definitions.
        /// </summary>
        public static class Constants
        {
            /// <summary>
            /// &quot;.&quot;
            /// </summary>
            public const string dot = ".";

            /// <summary>
            /// &quot;.g&quot;
            /// </summary>
            /// <see cref="dot"/>
            public static readonly string g = $"{dot}{nameof(g)}";

            /// <summary>
            /// &quot;.cs&quot;
            /// </summary>
            /// <see cref="dot"/>
            public static readonly string cs = $"{dot}{nameof(cs)}";
        }
        // ReSharper restore InconsistentNaming

        /// <summary>
        /// Renders the Generated File Name given the base <paramref name="uuid"/>.
        /// </summary>
        /// <param name="uuid"></param>
        /// <returns></returns>
        public static string RenderGeneratedFileName(this Guid uuid) => $"{uuid:D}{g}{cs}";
    }
}
