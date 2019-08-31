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
        /// Reports the <see cref="Exception"/> <paramref name="ex"/> through the
        /// <paramref name="writer"/>.
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="writer"></param>
        private static void ReportException(Exception ex, TextWriter writer)
        {
            while (ex != null)
            {
                // ReSharper disable once IdentifierTypo
                if (ex is CodeGenerationDependencyException cgdex)
                {
                    writer.WriteLine($"Path: {cgdex.Path}");
                }

                writer.WriteLine($"Message: {ex.Message}");
                writer.WriteLine($"Stack trace: {ex.StackTrace}");

                ex = ex.InnerException;
            }
        }

        /// <summary>
        /// The Main method Program entry point.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static int Main(string[] args)
        {
            var @out = Out;

            using (var toolConsoleManager = new ToolConsoleManager(@out, Error))
            {
                var errorLevel = DefaultErrorLevel;

                if (toolConsoleManager.TryParseOrShowHelp(args))
                {
                    try
                    {
                        toolConsoleManager.Run(out errorLevel);
                    }
                    catch (Exception ex)
                    {
                        ReportException(ex, @out);
                        throw;
                    }
                }

                return errorLevel;
            }
        }
    }
}
