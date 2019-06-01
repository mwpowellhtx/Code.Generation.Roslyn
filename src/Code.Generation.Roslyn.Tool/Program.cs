using System;

namespace Code.Generation.Roslyn
{
    using NConsole.Options;
    using static Console;

    internal static class Program
    {
        private static int Main(string[] args)
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
