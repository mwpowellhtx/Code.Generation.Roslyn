﻿// Copyright (c) Michael W. Powell. All rights reserved.
// Licensed under the GPLv3 license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;

namespace Code.Generation.Roslyn
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    [CodeGenerationAttribute(typeof(ReflexiveCodeGenerator))]
    [Conditional("CodeGeneration")]
    public class ReflexiveCodeGenerationByTypeAttribute : TestAttributeBase
    {
    }
}
