using System;
using System.Linq;
using System.Text;

namespace Code.Generation.Roslyn
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using static String;
    using static VerificationResult;

    /// <summary>
    /// 
    /// </summary>
    public static class DiagnosticFormatting
    {
        /// <summary>
        /// Callback provided to Format zero or more <paramref name="diagnostics"/>. Also given
        /// <paramref name="analyzer"/> for informational purposes.
        /// </summary>
        /// <param name="language"></param>
        /// <param name="analyzer"></param>
        /// <param name="diagnostics"></param>
        /// <returns></returns>
        public delegate string FormatDiagnosticsCallback(string language, DiagnosticAnalyzer analyzer, params Diagnostic[] diagnostics);

        /// <summary>
        /// The caller is free to provide his own <see cref="FormatDiagnosticsCallback"/>,
        /// however, most likely, the default formatter is sufficient for eighty to ninety
        /// percent of what you want to do.
        /// </summary>
        /// <param name="language"></param>
        /// <param name="analyzer"></param>
        /// <param name="diagnostics"></param>
        /// <returns></returns>
        internal static string DefaultDiagnosticFormatter(string language, DiagnosticAnalyzer analyzer, params Diagnostic[] diagnostics)
        {
            string FormatDiagnostic(Diagnostic diagnostic)
            {
                var sb = new StringBuilder();

                sb.AppendLine($"// {diagnostic}");

                var analyzerType = analyzer.GetType();
                var rules = analyzer.SupportedDiagnostics;

                // TODO: TBD: is this just a fancy way of selecting the "single" rule corresponding to the Diagnostic Id?
                foreach (var rule in rules.Where(x => x != null && x.Id == diagnostic.Id))
                {
                    var location = diagnostic.Location;

                    string message;

                    if (location == Location.None)
                    {
                        message = $"Global '{language}' result '{analyzerType.FullName}.{rule.Id}'.";
                    }
                    else
                    {
                        // Positively assert when Location IsInMetadata, as contrasted with !IsInSource.
                        if (location.IsInMetadata)
                        {
                            throw new BadFormatterLocationException($"Unhandled diagnostic verification in metadata locations: {diagnostic}.", BadLocation, diagnostic);
                        }

                        //try
                        //{
                        //    Assert.True(location.IsInSource);
                        //}
                        //catch (TrueException)
                        //{
                        //    OutputHelper.WriteLine(
                        //        "Test base does not currently handle diagnostics in metadata locations."
                        //        + $"Diagnostic in metadata: {diagnostic}");
                        //    throw;
                        //}

                        var linePosition = diagnostic.Location.GetLineSpan().StartLinePosition;

                        message = $"'{language}' result at "
                                  + $"line {linePosition.Line + 1} "
                                  + $"column {linePosition.Character + 1}"
                                  + $", rule '{analyzerType.FullName}.{rule.Id}')";
                    }

                    // TODO: TBD: we'll have to watch this, but I suspect we are formatting only a SINGLE rule/diagnostic pair here...
                    sb.AppendLine(message);
                }

                return $"{sb}";
            }

            return Join(",\r\n", diagnostics.Select(FormatDiagnostic));
        }
    }
}
