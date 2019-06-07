using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Code.Generation.Roslyn.Integration
{
    using static Constants;
    using static Domain;
    using static Enums;
    using static String;
    using static Strings;
    using AttributeRenderingOptionDictionary = Dictionary<string, object>;
    using IAttributeRenderingOptionDictionary = IDictionary<string, object>;

    public class TestCaseBundle : TestCaseFixtureBase
    {
        public Guid Id { get; } = Guid.NewGuid();

        internal string ProjectName => $"{Id:D}";

        internal string ProjectFileName => $@"{ProjectName}\{ProjectName}.csproj";

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
            using (var rs = FixtureType.Assembly.GetManifestResourceStream(resourcePath))
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

        protected bool TryInfluenceAttributeAnnotation(string fileName, AttributeAnnotationCallback callback)
        {
            var filePath = Path.Combine(ProjectName, fileName);

            string s;

            using (var fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var reader = new StreamReader(fs))
                {
                    s = reader.ReadToEnd();
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

        internal string GetFilePath(string fileName) => $@"{ProjectName}\{fileName}";

        internal string GetFilePath(ModuleKind module) => GetFilePath(GetFileName(module));

        public virtual void AddClassAnnotation<TAttribute>(ModuleKind module, IAttributeRenderingOptionDictionary options = null)
            where TAttribute : Attribute
        {
            var fileName = GetFileName(module);

            if (IsNullOrEmpty(fileName))
            {
                return;
            }

            // TODO: TBD: I dare say we might even be able to leverage regular expressions here...
            TryInfluenceAttributeAnnotation(fileName, s => s.Replace(PublicPartialClass
                , $"{this.RenderAttributeNotation<TAttribute>(options)}{CarriageReturnLineFeed}    {PublicPartialClass}"));
        }

        public virtual void AddInterfaceAnnotation<TAttribute>(ModuleKind module, IAttributeRenderingOptionDictionary options = null)
            where TAttribute : Attribute
        {
            var fileName = GetFileName(module);

            if (IsNullOrEmpty(fileName))
            {
                return;
            }

            // TODO: TBD: I dare say we might even be able to leverage regular expressions here...
            TryInfluenceAttributeAnnotation(fileName, s => s.Replace(PublicPartialInterface
                , $"{this.RenderAttributeNotation<TAttribute>(options)}{CarriageReturnLineFeed}    {PublicPartialInterface}"));
        }

        public virtual void AddOuterTypeNamespaceUsingStatement<T>(ModuleKind module)
        {
            var fileName = GetFileName(module);

            if (IsNullOrEmpty(fileName))
            {
                return;
            }

            var pattern = FooNamespace;

            TryInfluenceAttributeAnnotation(fileName, s => s.Replace(pattern
                , Join($"{CarriageReturnLineFeed}{CarriageReturnLineFeed}"
                    , $"{Using} {typeof(T).Namespace}{SemiColon}", pattern)));
        }

        public virtual void AddInnerTypeNamespaceUsingStatement<T>(ModuleKind module)
        {
            var fileName = GetFileName(module);

            if (IsNullOrEmpty(fileName))
            {
                return;
            }

            var pattern = $"{FooNamespace}{CarriageReturnLineFeed}{OpenCurlyBrace}";

            TryInfluenceAttributeAnnotation(fileName, s => s.Replace(pattern
                , Join("    ", $"{pattern}{CarriageReturnLineFeed}"
                    , $"{Using} {typeof(T).Namespace}{SemiColon}{CarriageReturnLineFeed}{CarriageReturnLineFeed}")));
        }

        public virtual void AddAssemblyAnnotation<TAttribute>(ModuleKind module, IAttributeRenderingOptionDictionary options = null)
            where TAttribute : Attribute
        {
            var fileName = GetFileName(module);

            if (IsNullOrEmpty(fileName))
            {
                return;
            }

            // ReSharper disable once RedundantEmptyObjectOrCollectionInitializer
            options = options ?? new AttributeRenderingOptionDictionary { };

            // Ensures that Rendering occurs WithAssemblyAttribute or rather AsAssemblyAttribute.
            options[assembly] = true;

            TryInfluenceAttributeAnnotation(fileName
                , s => $"{s}{this.RenderAttributeNotation<TAttribute>(options)}{CarriageReturnLineFeed}");
        }

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

            bool ModulesDoesContain(ModuleKind module) => modules.Contains(module);

            foreach (var resourceName in GetValues<ModuleKind>().Where(ModulesDoesContain).Select(GetFileName).Where(IsNotNullOrEmpty))
            {
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

        protected override void Dispose(bool disposing)
        {
            if (disposing && !IsDisposed)
            {
                TearDown(ProjectName);
            }

            base.Dispose(disposing);
        }
    }
}
