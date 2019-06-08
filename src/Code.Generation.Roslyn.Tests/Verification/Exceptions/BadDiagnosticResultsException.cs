using System.Collections.Generic;
using System.Linq;

namespace Code.Generation.Roslyn
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using static DiagnosticFormatting;

    /// <summary>
    /// 
    /// </summary>
    /// <inheritdoc />
    public sealed class BadDiagnosticResultsException : VerificationException
    {
        /// <summary>
        /// Gets the Language.
        /// </summary>
        public string Language { get; }

        /// <summary>
        /// Gets the Analyzer.
        /// </summary>
        public DiagnosticAnalyzer Analyzer { get; }

        /// <summary>
        /// Gets the Expected <see cref="DiagnosticResult"/> set.
        /// </summary>
        public IEnumerable<DiagnosticResult> ExpectedResults { get; }

        /// <summary>
        /// Gets the Actual <see cref="Diagnostic"/> set.
        /// </summary>
        public IEnumerable<Diagnostic> ActualDiagnostics { get; }

        /// <summary>
        /// Gets or sets the <see cref="Diagnostic"/> Formatter. The default formatter is
        /// <see cref="DefaultDiagnosticFormatter"/>.
        /// </summary>
        /// <see cref="DefaultDiagnosticFormatter"/>
        public FormatDiagnosticsCallback DiagnosticFormatter { get; set; }

        internal BadDiagnosticResultsException(string message, string language, DiagnosticAnalyzer analyzer
            , VerificationResult result, DiagnosticResult expectedResult, Diagnostic actualDiagnostic)
            : this(message, language, analyzer, result, new[] { expectedResult }, new[] { actualDiagnostic })
        {
        }

        internal BadDiagnosticResultsException(string message, string language, DiagnosticAnalyzer analyzer
            , VerificationResult result, IEnumerable<DiagnosticResult> expectedResults
            , IEnumerable<Diagnostic> actualDiagnostics)
            : base(message, result)
        {
            Language = language;
            Analyzer = analyzer;
            ExpectedResults = expectedResults.ToArray();
            ActualDiagnostics = actualDiagnostics.ToArray();
            DiagnosticFormatter = DefaultDiagnosticFormatter;
        }
    }
}
