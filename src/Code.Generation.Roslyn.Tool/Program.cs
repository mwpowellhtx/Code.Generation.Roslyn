using System;
using System.IO;

namespace Code.Generation.Roslyn
{
    using NConsole.Options;

    internal static class Program
    {
        /// <summary>
        /// Gets the Out <see cref="TextWriter"/>.
        /// </summary>
        /// <remarks>Allows for Internal Set for unit test purposes.</remarks>
        public static TextWriter Out { get; internal set; } = Console.Out;

        /// <summary>
        /// Gets the Error <see cref="TextWriter"/>.
        /// </summary>
        /// <remarks>Allows for Internal Set for unit test purposes.</remarks>
        public static TextWriter Error { get; internal set; } = Console.Error;

        /// <summary>
        /// <see cref="ConsoleManager.DefaultErrorLevel"/>
        /// </summary>
        internal const int DefaultErrorLevel = ConsoleManager.DefaultErrorLevel;

        /// <summary>
        /// The Main method Program entry point.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static int Main(string[] args)
        {
            using (var toolConsoleManager = new ToolConsoleManager(Out, Error))
            {
                var errorLevel = DefaultErrorLevel;

                if (toolConsoleManager.TryParseOrShowHelp(args))
                {
                    toolConsoleManager.Run(out errorLevel);
                }

                return errorLevel;
            }
        }
    }
}
