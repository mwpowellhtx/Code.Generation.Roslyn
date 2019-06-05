using System;

namespace Code.Generation.Roslyn.Integration
{
    [Flags]
    public enum ModuleKind
    {
        Foo = 1,
        Bar = 1 << 1,
        Baz = 1 << 2,
        Biz = 1 << 3,
        AssemblyInfo = 1 << 5
    }
}
