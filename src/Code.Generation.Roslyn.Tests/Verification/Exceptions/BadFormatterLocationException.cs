namespace Code.Generation.Roslyn
{
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// 
    /// </summary>
    /// <inheritdoc />
    public sealed class BadFormatterLocationException : VerificationException
    {
        /// <summary>
        /// Gets the Diagnostic associated with the Exception.
        /// </summary>
        public Diagnostic Diagnostic { get; }

        internal BadFormatterLocationException(string message, VerificationResult result, Diagnostic diagnostic)
            : base(message, result)
        {
            Diagnostic = diagnostic;
        }
    }
}
