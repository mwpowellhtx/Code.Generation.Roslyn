namespace Code.Generation.Roslyn
{
    public class AssemblyDescriptor
    {
        internal static AssemblyDescriptor Create(string assemblyPath)
            => new AssemblyDescriptor {AssemblyPath = assemblyPath};

        public string AssemblyPath { get; set; }
    }
}
