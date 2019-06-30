// Copyright (c) Michael W. Powell. All rights reserved.
// Licensed under the GPLv3 license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;

namespace Code.Generation.Roslyn.Generators
{
    [AttributeUsage(AttributeTargets.Assembly)]
    [CodeGenerationAttribute(typeof(FruitPlantationGenerator))]
    [Conditional("CodeGeneration")]
    public class FruitPlantationAttribute : TestAttributeBase
    {
    }
}
