using System;
using System.IO;
using System.Linq;

namespace Code.Generation.Roslyn.Integration
{
    using static Resources;
    using static ModuleKind;

    public class TestCaseBundle : IDisposable
    {
        protected Type BundleType { get; }

        public Guid Id { get; } = Guid.NewGuid();

        internal string ProjectName => $"{Id:D}";

        internal string ProjectFileName => $@"{ProjectName}\{ProjectName}.csproj";

        public TestCaseBundle()
        {
            BundleType = GetType();
        }

        protected bool TryEnsureDirectoryExists(string projectName)
        {
            if (!Directory.Exists(projectName))
            {
                Directory.CreateDirectory(projectName);
            }

            return Directory.Exists(projectName);
        }

        protected bool TryEnsureFileExists(string resourcePath, string filePath, int blockSize = 16)
        {
            // This is a GREAT use of a Local Function.
            bool TryReadBuffer(BinaryReader reader, ref byte[] buffer, out int actualCount)
                => (actualCount = reader.Read(buffer, 0, buffer.Length)) > 0;

            // TODO: TBD: or we could "pipe" bytes from the stream if memory blocks are an issue...
            using (var rs = BundleType.Assembly.GetManifestResourceStream(resourcePath))
            {
                using (var reader = new BinaryReader(rs))
                {
                    using (var fs = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.Read))
                    {
                        using (var writer = new BinaryWriter(fs))
                        {
                            var buffer = new byte[blockSize];

                            while (TryReadBuffer(reader, ref buffer, out var actualSize))
                            {
                                var current = buffer.Take(actualSize).ToArray();
                                writer.Write(current);
                            }
                        }
                    }
                }
            }

            return true;
        }

        public delegate string AttributeAnnotationCallback(string s);

        protected bool TryInfluenceAttributeAnnotation(string fileName, AttributeAnnotationCallback callback)
        {
            var filePath = Path.Combine(ProjectName, fileName);

            string s;

            using (var fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var reader = new StreamReader(fs))
                {
                    s = reader.ReadToEnd();
                    callback(s);
                }
            }

            // The proper response here is a bit nuanced.
            var x = callback(s).Trim();

            if (x == null)
            {
                return false;
            }

            if (!x.Any())
            {
                // The great thing is `removing´ a file from the project is tantamount to simply deleting it.
                File.Delete(filePath);
                return true;
            }

            // Yes, `Create´ here... Because we may want to Truncate the result.
            using (var fs = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                using (var writer = new StreamWriter(fs))
                {
                    writer.Write(x);
                }
            }

            return s != x;
        }

        public void RemoveClassAnnotation<TAttribute>(ModuleKind module)
            where TAttribute : Attribute
        {
            var fileName = GetFileName(module);

            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }

            // TODO: TBD: arguably, could we furnish a code analyzer/fixer around this issue?
            const string carriageReturnLineFeed = "\r\n";
            const string publicPartialClass = "    public partial class";

            // TODO: TBD: I dare say we might even be able to leverage regular expressions here...
            TryInfluenceAttributeAnnotation(fileName, s => s.Replace(
                $"[{nameof(Attribute)}]{carriageReturnLineFeed}{publicPartialClass}"
                , publicPartialClass.Trim())
            );
        }

        public void RemoveClassAnnotation(ModuleKind module) => RemoveClassAnnotation<Attribute>(module);

        public void RemoveInterfaceAnnotation<TAttribute>(ModuleKind module)
            where TAttribute : Attribute
        {
            var fileName = GetFileName(module);

            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }

            // TODO: TBD: arguably, could we furnish a code analyzer/fixer around this issue?
            const string carriageReturnLineFeed = "\r\n";
            const string publicPartialInterface = "    public partial interface";

            // TODO: TBD: I dare say we might even be able to leverage regular expressions here...
            TryInfluenceAttributeAnnotation(fileName, s => s.Replace(
                $"[{nameof(Attribute)}]{carriageReturnLineFeed}{publicPartialInterface}"
                , publicPartialInterface.Trim())
            );
        }

        public void RemoveInterfaceAnnotation(ModuleKind module) => RemoveInterfaceAnnotation<Attribute>(module);

        public void RemoveAssemblyAnnotation<TAttribute>(ModuleKind module)
            where TAttribute : Attribute
        {
            var fileName = GetFileName(module);

            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }

            // TODO: TBD: I dare say we might even be able to leverage regular expressions here...
            TryInfluenceAttributeAnnotation(fileName, s => s.Replace($"[assembly: {nameof(Attribute)}]", ""));
        }

        public void RemoveAssemblyAnnotation(ModuleKind module) => RemoveAssemblyAnnotation<Attribute>(module);

        protected string GetFileName(ModuleKind module)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (module)
            {
                case Bar:
                case Baz:
                case AssemblyInfo:
                    return $"{module}.cs";

                case Biz:
                    return $"I{module}.cs";
            }

            return null;
        }

        internal string GetFilePath(string fileName) => $@"{ProjectName}\{fileName}";

        internal string GetFilePath(ModuleKind module) => GetFilePath(GetFileName(module));

        protected string GetBundledResourcePath(string resourceName) => Combine(BundleType.Namespace, resourceName);

        // TODO: TBD: we call this 'Extrapolate' because there may be additional work we want to do on the files/text themselves...
        // TODO: TBD: i.e. getting setup for the code generation unit tests... introducing Using statements, attribute annotations, etc
        /// <summary>
        /// Extrapolates the <paramref name="modules"/> and such informing the project.
        /// </summary>
        /// <param name="modules"></param>
        public virtual void Extrapolate(ModuleKind? modules)
        {
            var projectName = ProjectName;

            TryEnsureDirectoryExists(projectName);

            TryEnsureFileExists(GetBundledResourcePath("Project.Template.xml"), ProjectFileName);

            // TODO: TBD: extending a ToArray, etc, from Array might be interesting...
            foreach (var module in Enum.GetValues(typeof(ModuleKind)).ToArray<ModuleKind>())
            {
                if (!modules.Contains(module))
                {
                    continue;
                }

                var resourceName = GetFileName(module);

                // TODO: TBD: we SHOULD have a value here...
                // TODO: TBD: anything less or other than that is stronger than continuing I think, possibly even an exception...
                if (string.IsNullOrEmpty(resourceName))
                {
                    continue;
                }

                TryEnsureFileExists(GetBundledResourcePath($"{resourceName}"), $@"{projectName}\{resourceName}");
            }
        }

        public virtual void TearDown(string projectName)
        {
            if (!Directory.Exists(projectName))
            {
                return;
            }

            Directory.Delete(projectName, true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing || IsDisposed)
            {
                return;
            }

            TearDown(ProjectName);
        }

        protected bool IsDisposed { get; private set; }

        public void Dispose()
        {
            Dispose(true);
            IsDisposed = true;
        }
    }
}
