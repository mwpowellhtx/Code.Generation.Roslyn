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
        public static TextWriter Out { get; set; } = Console.Out;

        /// <summary>
        /// Gets the Error <see cref="TextWriter"/>.
        /// </summary>
        public static TextWriter Error { get; set; } = Console.Error;

        /// <summary>
        /// The Main method Program entry point.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static int Main(string[] args)
        {
            using (var toolConsoleManager = new ToolConsoleManager(Out, Error))
            {
                var errorLevel = ConsoleManager.DefaultErrorLevel;

                if (toolConsoleManager.TryParseOrShowHelp(args))
                {
                    toolConsoleManager.Run(out errorLevel);
                }

                return errorLevel;
            }
        }
    }
}
