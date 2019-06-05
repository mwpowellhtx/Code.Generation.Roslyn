// Copyright (c) Michael W. Powell. All rights reserved.
// Licensed under the GPPLv3 license. See LICENSE file in the project root for full license information.

namespace Code.Generation.Roslyn.Tests.Generators
{
    using System;
    using System.Diagnostics;

    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    [CodeGenerationAttribute(typeof(DuplicateInOtherNamespaceCodeGenerator))]
    [Conditional("CodeGeneration")]
    public class DuplicateInOtherNamespaceAttribute : TestAttributeBase
    {
        public DuplicateInOtherNamespaceAttribute(string @namespace)
        {
            Namespace = @namespace;
        }

        public string Namespace { get; }
    }
}