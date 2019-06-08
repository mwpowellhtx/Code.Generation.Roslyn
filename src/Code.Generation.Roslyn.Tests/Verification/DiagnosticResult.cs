using System;
using System.Collections.Generic;
using System.Linq;

namespace Code.Generation.Roslyn
{
    using Microsoft.CodeAnalysis;
    using static String;

    /// <summary>
    /// Struct that stores information about a Diagnostic appearing in a source.
    /// </summary>
    public struct DiagnosticResult
    {
        private IEnumerable<DiagnosticResultLocation> _locations;

        /// <summary>
        /// Gets the Locations.
        /// </summary>
        public IEnumerable<DiagnosticResultLocation> Locations
        {
            get => _locations ?? (_locations = new DiagnosticResultLocation[0]);
            private set => _locations = value;
        }

        /// <summary>
        /// Gets the Severity.
        /// </summary>
        public DiagnosticSeverity Severity { get; }

        /// <summary>
        /// Gets the Id.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets the Message.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Public Constructor.
        /// </summary>
        /// <param name="diagnostic"></param>
        private DiagnosticResult(Diagnostic diagnostic)
        {
            Id = diagnostic.Id;
            // TODO: TBD: how do we know which arguments to present to the to the MessageFormat string?
            Message = diagnostic.GetMessage();
            Severity = diagnostic.Severity;
            _locations = null;
        }

        /// <summary>
        /// Returns the Created <see cref="DiagnosticResult"/> given <paramref name="diagnostic"/>
        /// and <paramref name="locations"/>.
        /// </summary>
        /// <param name="diagnostic"></param>
        /// <param name="locations"></param>
        /// <returns></returns>
        public static DiagnosticResult Create(Diagnostic diagnostic, params DiagnosticResultLocation[] locations)
            => new DiagnosticResult(diagnostic) {Locations = locations};
        // TODO: TBD: may round out the Create methods...

        /// <summary>
        /// Gets the Path based on the <see cref="Locations"/>.
        /// </summary>
        internal string Path => Locations.Any() ? Locations.First().Path : Empty;

        /// <summary>
        /// Gets the Line based on the <see cref="Locations"/>.
        /// </summary>
        internal int Line => Locations.Any() ? Locations.First().Line : -1;

        /// <summary>
        /// Gets the Column based on the <see cref="Locations"/>.
        /// </summary>
        internal int Column => Locations.Any() ? Locations.First().Column : -1;

        /// <summary>
        /// Gets a Rendered Summary from the Result.
        /// </summary>
        internal string Summary
        {
            get
            {
                bool TryRenderValue(string name, int value, out string rendered)
                    => (rendered = value < 0 ? Empty : $"{name.ToLower()} {value}").Any();

                string RenderLineAndColumn(int line, int column)
                    => TryRenderValue(nameof(line), line, out var x)
                       && TryRenderValue(nameof(column), column, out var y)
                        ? $" ({x}, {y})"
                        : Empty;

                var renderedLineAndColumn = RenderLineAndColumn(Line, Column);

                return IsNullOrEmpty(Path)
                    ? $"[{Id}]: {Message}{renderedLineAndColumn}"
                    : $"[{Id}] {Path}: {Message}{renderedLineAndColumn}";
            }
        }
    }
}
