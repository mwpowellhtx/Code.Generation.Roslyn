// Copyright (c) Michael W. Powell. All rights reserved.
// Licensed under the GPLv3 license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;

namespace Code.Generation.Roslyn
{
    using Validation;

    /// <inheritdoc />
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = true)]
    [CodeGenerationAttribute("Code.Generation.Roslyn.Tests.Generators.DuplicateWithSuffixCodeGenerator, Code.Generation.Roslyn.Generators")]
    [Conditional("CodeGeneration")]
    public class DuplicateWithSuffixByNameAttribute : TestAttributeBase
    {
        /// <summary>
        /// Gets the Suffix.
        /// </summary>
        public string Suffix { get; }

        /// <summary>
        /// Public Constructor.
        /// </summary>
        /// <param name="suffix"></param>
        /// <inheritdoc />
        public DuplicateWithSuffixByNameAttribute(string suffix)
        {
            Requires.NotNullOrEmpty(suffix, nameof(suffix));

            Suffix = suffix;
        }
    }
}
