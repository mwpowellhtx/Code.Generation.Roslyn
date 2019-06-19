using System;
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
        private Guid? ResponseId { get; set; }

        /// <summary>
        /// Gets or Sets whether UsingResponseFile.
        /// </summary>
        internal bool UsingResponseFile
        {
            get => ResponseId.HasValue;
            set => ResponseId = value ? Guid.NewGuid() : (Guid?) null;
        }

        /// <summary>
        /// Gets the Response File Path contingent upon whether <see cref="UsingResponseFile"/>.
        /// The full path is also contingent upon <see cref="Output"/> having been provided.
        /// </summary>
        /// <see cref="UsingResponseFile"/>
        /// <see cref="Output"/>
        /// <see cref="ResponseId"/>
        private string Response => UsingResponseFile ? $"{Combine(Output, $"{ResponseId:D}.rsp")}" : null;

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
        /// Adds <paramref name="values"/> to the <see cref="Sources"/>. Sources would ordinarily
        /// be discovered during the Code Generation GenerateCodeFromAttributes Microsoft Build
        /// step. Following which they are tallied by the Targets
        /// CodeGenCompilationInputsFromAttributes property. However, it is necessary to specify
        /// them manually for integration testing purposes.
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
        /// Adds <paramref name="values"/> to the <see cref="Defines"/>. Similarly as with
        /// Sources, Defines are tallied by the DefineConstants Microsoft Build property.
        /// However, they must be manually specified during integration testing.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        internal ToolingParameterBuilder AddDefines(params string[] values)
        {
            values.ToList().ForEach(Defines.Add);
            return this;
        }

        /// <summary>
        /// Enumerates the Response File only Command Line arguments for purposes of internal
        /// unit testing.
        /// </summary>
        /// <returns></returns>
        /// <remarks>The naming conventions herein are not an accident. They are carefully
        /// chosen and aligned with the tooling command line argument prototypes.</remarks>
        private IEnumerable<string> GetResponseFileToolingParameters()
        {
            yield return $"{DoubleDash}{nameof(Response).ToLower()}";
            yield return $"{Response}";
        }

        /// <summary>
        /// Enumerates the Command Line Arguments for purposes of internal unit testing.
        /// As it happens, the enumerated arguments serve as the actual command line
        /// arguments as well as those relayed through a <see cref="Response"/> file.
        /// </summary>
        /// <returns></returns>
        /// <remarks>The naming conventions herein are not an accident. They are carefully
        /// chosen and aligned with the tooling command line argument prototypes.</remarks>
        private IEnumerable<string> GetCommandLineToolingParameters()
        {
            yield return $"{DoubleDash}{nameof(Project).ToLower()}";
            yield return $"{Project}";

            yield return $"{DoubleDash}{nameof(Output).ToLower()}";
            yield return $"{Output}";

            yield return $"{DoubleDash}{nameof(Generated).ToLower()}";
            yield return $"{Generated}";

            yield return $"{DoubleDash}{nameof(Assemblies).ToLower()}";
            yield return $"{Assemblies}";

            foreach (var source in Sources)
            {
                yield return $"{DoubleDash}{nameof(source)}";
                yield return source;
            }

            foreach (var define in Defines)
            {
                yield return $"{DoubleDash}{nameof(define)}";
                yield return define;
            }

            foreach (var reference in References)
            {
                yield return $"{DoubleDash}{nameof(reference)}";
                yield return reference;
            }
        }

        /// <summary>
        /// Returns the Appropriate set of Command Line Parameters contingent on whether
        /// <see cref="UsingResponseFile"/>. Which also requires for <see cref="Output"/>
        /// to have been specified in any event. <see cref="Response"/> will also be provided
        /// as a function of whether <see cref="UsingResponseFile"/>.
        /// </summary>
        /// <returns></returns>
        /// <see cref="Output"/>
        /// <see cref="Response"/>
        /// <see cref="UsingResponseFile"/>
        /// <see cref="GetCommandLineToolingParameters"/>
        /// <see cref="GetResponseFileToolingParameters"/>
        private IEnumerable<string> GetResponseFileOrCommandLineParameters()
        {
            var args = GetCommandLineToolingParameters().ToArray();

            if (!UsingResponseFile)
            {
                return args;
            }

            // Remember to do a little bookkeeping along the way.
            void EnsureOutputDirectoryExists(string output)
            {
                if (Directory.Exists(output))
                {
                    return;
                }

                Directory.CreateDirectory(output);
            }

            EnsureOutputDirectoryExists(Output);

            using (var s = File.Open(Response, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                using (var writer = new StreamWriter(s))
                {
                    foreach (var arg in args)
                    {
                        writer.WriteLine(arg.ToArray());
                    }
                }
            }

            return GetResponseFileToolingParameters();
        }

        public IEnumerator<string> GetEnumerator() => GetResponseFileOrCommandLineParameters().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
