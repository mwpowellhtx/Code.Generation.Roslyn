using System.IO;

namespace Code.Generation.Roslyn.Logging
{
    internal delegate void LoggerCallback(TextWriter writer, int logLevel, string formattedMessage, string diagnosticCode = null);
}
