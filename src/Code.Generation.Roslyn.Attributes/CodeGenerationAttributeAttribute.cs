// TODO: TBD: decide what kind of licensing to run with here? MS-PL? Apache?
// Copyright (c) 2019 Michael W Powell. All rights reserved.
// Licensed under the MS-PL license. See LICENSE.txt file in the project root for full license information.

using System;

namespace Code.Generation.Roslyn
{
    using static AttributeTargets;

    /// <summary>
    /// A base attribute type for code generation attributes.
    /// </summary>
    /// <inheritdoc />
    [AttributeUsage(Class | Struct | Interface | Enum | Module | Assembly, Inherited = false)]
    public class CodeGenerationAttributeAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CodeGenerationAttributeAttribute"/> class.
        /// </summary>
        /// <param name="generatorFullTypeName">The fully-qualified type name (including
        /// assembly information) of the code generator to activate. This type must implement
        /// ICodeGenerator.</param>
        /// <inheritdoc />
        public CodeGenerationAttributeAttribute(string generatorFullTypeName)
        {
            GeneratorFullTypeName = generatorFullTypeName;
        }

        /// <summary>
        /// Returns the <see cref="Type.AssemblyQualifiedName"/> from the
        /// <paramref name="generatorType"/> while also verifying an actual
        /// instance is there.
        /// </summary>
        /// <param name="generatorType"></param>
        /// <returns></returns>
        private static string GetAssemblyQualifiedName(Type generatorType)
        {
            // TODO: TBD: almost beggars the question, is it appropriate to adopt a Validation dependency?
            if (generatorType == null)
            {
                throw new ArgumentNullException(nameof(generatorType));
            }

            return generatorType.AssemblyQualifiedName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeGenerationAttributeAttribute"/> class.
        /// </summary>
        /// <param name="generatorType">The code generator that implements ICodeGenerator.</param>
        /// <inheritdoc />
        public CodeGenerationAttributeAttribute(Type generatorType)
            : this(GetAssemblyQualifiedName(generatorType))
        {
        }

        /// <summary>
        /// Gets the fully-qualified type name, including assembly information,
        /// of the code generator to activate.
        /// </summary>
        public string GeneratorFullTypeName { get; }
    }
}
