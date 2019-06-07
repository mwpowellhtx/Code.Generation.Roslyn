using System;

namespace Code.Generation.Roslyn.Integration
{
    /// <summary>
    /// Represents a handful of internal monikers useful for interfacing with embedded
    /// resources, extrapolation, file and string manipulation, and so on.
    /// </summary>
    [Flags]
    public enum ModuleKind
    {
        /// <summary>
        /// The internal moniker for the Namespace.
        /// </summary>
        Foo = 1,

        /// <summary>
        /// The internal moniker for a Class.
        /// </summary>
        Bar = 1 << 1,

        /// <summary>
        /// The internal moniker for another Class deriving from the <see cref="Bar"/> Class.
        /// </summary>
        Baz = 1 << 2,

        /// <summary>
        /// Biz is the internal moniker for the Interface.
        /// </summary>
        Biz = 1 << 3,

        /// <summary>
        /// The internal moniker for all things AssemblyInfo.
        /// </summary>
        AssemblyInfo = 1 << 5
    }
}
