namespace Code.Generation.Roslyn
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using static DiagnosticFormatting;

    /// <summary>
    /// 
    /// </summary>
    /// <inheritdoc />
    public sealed class BadDiagnosticLocationException : VerificationException
    {
        /// <summary>
        /// Gets the Language.
        /// </summary>
        public string Language { get; }

        /// <summary>
        /// Gets the <see cref="DiagnosticAnalyzer"/>.
        /// </summary>
        public DiagnosticAnalyzer Analyzer { get; }

        /// <summary>
        /// 
        /// </summary>
        public DiagnosticResultLocation ExpectedResultLocation { get; }

        /// <summary>
        /// 
        /// </summary>
        public Diagnostic ActualDiagnostic { get; }

        /// <summary>
        /// 
        /// </summary>
        public Location ActualLocation { get; }

        /// <summary>
        /// Gets or sets the <see cref="Diagnostic"/> Formatter. The default formatter is
        /// <see cref="DefaultDiagnosticFormatter"/>.
        /// </summary>
        /// <see cref="DefaultDiagnosticFormatter"/>
        public FormatDiagnosticsCallback DiagnosticFormatter { get; set; }

        internal BadDiagnosticLocationException(string message, string language, DiagnosticAnalyzer analyzer
            , VerificationResult result, DiagnosticResultLocation expectedResultLocation
            , Diagnostic actualDiagnostic, Location actualLocation)
            : base(message, result)
        {
            Language = language;
            Analyzer = analyzer;
            ExpectedResultLocation = expectedResultLocation;
            ActualDiagnostic = actualDiagnostic;
            ActualLocation = actualLocation;
        }
    }
}
