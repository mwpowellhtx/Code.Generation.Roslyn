using System.Collections.Generic;

namespace Code.Generation.Roslyn
{
    internal interface IToolingParameterBuilder : IEnumerable<string>
    {
    }

    internal interface IToolingParameterBuilder<out TParent> : IToolingParameterBuilder, ICanReferenceAssembly<TParent>
    {
    }
}
