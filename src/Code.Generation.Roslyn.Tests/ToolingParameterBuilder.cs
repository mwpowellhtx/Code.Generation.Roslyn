using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Code.Generation.Roslyn
{
    using static Domain;
    using static Path;

    internal class ToolingParameterBuilder : IToolingParameterBuilder<ToolingParameterBuilder>
    {
        internal string Project { get; set; }

        private string _output;

        /// <summary>
        /// Gets or Sets the Output Parameter.
        /// Defaults to &quot;<see cref="Project"/>\obj&quot;.
        /// </summary>
        internal string Output
        {
            get => _output ?? $@"{Project}\obj";
            set => _output = value;
        }

        private string _generated;

        /// <summary>
        /// Gets or Sets the Generated Parameter.
        /// Defaults to &quot;<see cref="Project"/>.g.json&quot;.
        /// </summary>
        internal string Generated
        {
            get => _generated ?? $@"{Project}.g.json";
            set => _generated = value;
        }

        private string _assemblies;

        /// <summary>
        /// Gets or Sets the Assemblies Parameter.
        /// Defaults to &quot;<see cref="Project"/>.a.json&quot;.
        /// </summary>
        internal string Assemblies
        {
            get => _assemblies ?? $@"{Project}.a.json";
            set => _assemblies = value;
        }

        // ReSharper disable once RedundantEmptyObjectOrCollectionInitializer
        /// <summary>
        /// Gets the References.
        /// </summary>
        internal ICollection<string> References { get; } = new List<string> { };

        /// <summary>
        /// Adds the Reference <paramref name="path"/> and returns itself.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private ToolingParameterBuilder AddReference(string path)
        {
            References.Add(path);
            return this;
        }

        /// <inheritdoc />
        public ToolingParameterBuilder AddTypeAssemblyLocationBasedReferences<T>(params string[] assetRelativeNames)
        {
            var directoryName = GetDirectoryName(typeof(T).Assembly.Location);
            return assetRelativeNames.Aggregate(this, (g, x) => g.AddReference(Combine(directoryName, x)));
        }

        /// <inheritdoc />
        public ToolingParameterBuilder AddReferenceToTypeAssembly<T>() => AddReference(typeof(T).Assembly.Location);

        /// <inheritdoc />
        public ToolingParameterBuilder AddReferencesByPaths(params string[] paths) => paths.Aggregate(this, (x, p) => x.AddReference(p));

        // ReSharper disable once RedundantEmptyObjectOrCollectionInitializer
        /// <summary>
        /// Gets the Sources.
        /// </summary>
        internal ICollection<string> Sources { get; } = new List<string> { };

        /// <summary>
        /// Adds <paramref name="values"/> to the <see cref="Sources"/>.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        internal ToolingParameterBuilder AddSources(params string[] values)
        {
            values.ToList().ForEach(Sources.Add);
            return this;
        }

        // ReSharper disable once RedundantEmptyObjectOrCollectionInitializer
        /// <summary>
        /// Gets the Defines.
        /// </summary>
        internal ICollection<string> Defines { get; } = new List<string> { };

        /// <summary>
        /// Adds <paramref name="values"/> to the <see cref="Defines"/>.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        internal ToolingParameterBuilder AddDefines(params string[] values)
        {
            values.ToList().ForEach(Defines.Add);
            return this;
        }

        /// <summary>
        /// Enumerates the Command Line Arguments for purposes of internal unit testing.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<string> GetToolingParameters()
        {
            yield return $@"{DoubleDash}{nameof(Project).ToLower()}";
            yield return $@"{Project}";

            yield return $@"{DoubleDash}{nameof(Output).ToLower()}";
            yield return $@"{Output}";

            yield return $@"{DoubleDash}{nameof(Generated).ToLower()}";
            yield return $@"{Generated}";

            yield return $@"{DoubleDash}{nameof(Assemblies).ToLower()}";
            yield return $@"{Assemblies}";

            foreach (var source in Sources)
            {
                yield return $@"{DoubleDash}{nameof(source)}";
                yield return source;
            }

            foreach (var define in Defines)
            {
                yield return $@"{DoubleDash}{nameof(define)}";
                yield return define;
            }

            foreach (var reference in References)
            {
                yield return $@"{DoubleDash}{nameof(reference)}";
                yield return reference;
            }
        }

        public IEnumerator<string> GetEnumerator() => GetToolingParameters().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
