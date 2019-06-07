// Copyright (c) Michael W. Powell. All rights reserved.
// Licensed under the GPLv3 license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;

namespace Code.Generation.Roslyn
{
    /// <inheritdoc />
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = true)]
    [CodeGenerationAttribute("Code.Generation.Roslyn.Tests.Generators.ReflexiveCodeGenerator, Code.Generation.Roslyn.Generators")]
    [Conditional("CodeGeneration")]
    public class ReflexiveCodeGenerationByNameAttribute : TestAttributeBase
    {
    }
}
