// Copyright (c) Michael W. Powell. All rights reserved.
// Licensed under the GPLv3 license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;

namespace Code.Generation.Roslyn
{
    using Validation;

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    [CodeGenerationAttribute(typeof(DuplicateWithSuffixCodeGenerator))]
    [Conditional("CodeGeneration")]
    public class DuplicateWithSuffixByTypeAttribute : TestAttributeBase
    {
        public DuplicateWithSuffixByTypeAttribute(string suffix)
        {
            Requires.NotNullOrEmpty(suffix, nameof(suffix));

            this.Suffix = suffix;
        }

        public string Suffix { get; }
    }
}
