using System;

namespace Code.Generation.Roslyn
{
    /// <summary>
    /// 
    /// </summary>
    /// <inheritdoc />
    public abstract class VerificationException : Exception
    {
        /// <summary>
        /// Gets the Result.
        /// </summary>
        public VerificationResult Result { get; }

        /// <summary>
        /// Protected Constructor
        /// </summary>
        /// <param name="message"></param>
        /// <param name="result"></param>
        /// <inheritdoc />
        protected VerificationException(string message, VerificationResult result)
            : base(message)
        {
            Result = result;
        }
    }
}
