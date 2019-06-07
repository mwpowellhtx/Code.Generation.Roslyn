// Copyright (c) Michael W. Powell. All rights reserved.
// Licensed under the GPPLv3 license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Code.Generation.Roslyn.Tests.Generators
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    public class DuplicateInOtherNamespaceCodeGenerator : CodeGeneratorBase
    {
        public DuplicateInOtherNamespaceCodeGenerator(AttributeData attributeData)
            : base(attributeData)
        {
        }

        public override Task GenerateAsync(TransformationContext context, IProgress<Diagnostic> progress
            , CancellationToken cancellationToken)
        {
            var results = SyntaxFactory.List<MemberDeclarationSyntax>();

            if (!(context.ProcessingNode is ClassDeclarationSyntax classDecl))
            {
                // return
            }



            //var partial = SyntaxFactory.ClassDeclaration(classDeclaration.Identifier);
            //var namespaceSyntax =
            //    SyntaxFactory.NamespaceDeclaration(
            //            SyntaxFactory.ParseName(Namespace))
            //        .AddMembers(partial);

            //context.AddMember(namespaceSyntax);

            return Task.FromResult(results);
        }
    }
}