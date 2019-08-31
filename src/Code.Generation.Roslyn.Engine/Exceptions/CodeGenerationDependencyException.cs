using System;

namespace Code.Generation.Roslyn
{
    public class CodeGenerationDependencyException : CodeGenerationException
    {
        public string Path { get; }

        public CodeGenerationDependencyException(string message, string path)
            : base(message)
        {
            Path = path;
        }

        public CodeGenerationDependencyException(string message, string path, Exception innerException)
            : base(message, innerException)
        {
            Path = path;
        }
    }
}
