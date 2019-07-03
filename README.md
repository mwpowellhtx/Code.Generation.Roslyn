
# Roslyn-based Code Generation

[![Code.Generation.Roslyn NuGet package](https://img.shields.io/nuget/v/Code.Generation.Roslyn.svg?label=Code.Generation.Roslyn%20NuGet%20Package)][NuPkg]
[![Code.Generation.Roslyn.Attributes NuGet package](https://img.shields.io/nuget/v/Code.Generation.Roslyn.Attributes.svg?label=Code.Generation.Roslyn.Attributes%20NuGet%20Package)][AttrNuPkg]
[![Code.Generation.Roslyn.Engine NuGet package](https://img.shields.io/nuget/v/Code.Generation.Roslyn.Engine.svg?label=Code.Generation.Roslyn.Engine%20NuGet%20Package)][EngineNuPkg]
[![Code.Generation.Roslyn.BuildTime NuGet package](https://img.shields.io/nuget/v/Code.Generation.Roslyn.BuildTime.svg?label=Code.Generation.Roslyn.BuildTime%20NuGet%20Package)][BuildTimeNuPkg]
[![dotnet-cgr NuGet package](https://img.shields.io/nuget/v/dotnet-cgr.svg?label=dotnet-cgr%20NuGet%20Package)][ToolNuPkg]

## Background &amp; Motivations

Before we dive into the finer points of Code Generation, it is necessary to discuss a couple of key differences separating this effort from the original effort upon which our inspiration was derived.

First and foremost, kudos to [Andrew Arnott](https://github.com/AArnott) and folks for the original effort. We have been able to generate code in most circumstances, but lately there are a couple of requirements we had in which [CodeGeneration.Roslyn](https://github.com/AArnott/CodeGeneration.Roslyn) just was not going to work for us. This is the primary motivation for our recasting of the original effort, distilling just the code generation facilitation and getting out of the way of the code generation author as quickly as possible. We do this in a couple of ways:

1. First, [Code.Generation.Roslyn](/mwpowellhtx/Code.Generation.Roslyn) is focused on just that: facilitating code generation. What do we mean by that? To be clear, we found it to be extraneous, and a bit of a distraction, to consider whether the annotated request originated in a class, struct, in what name spaces, and so on. Instead, once we discover the annotation, we simply relay that to the code generation author and step out of the way as early as possible. We do this by requiring code be generated at the CompilationUnitSyntax level as contrasted with the MemberDeclarationSyntax level.
1. Secondly, we wanted to generated code triggered by Assembly Attributes, not just Member, i.e. *class*, *struct*, *interface*, etc, Attributes.
1. Thirdly, a tertiary goal of ours was to allow for custom *Preamble Text* to be delivered into the engine. This is required to be properly formatted, however, the engine itself will append a trailing Carriage Return Line Feed symbol as appropriate. The default, of course, is a predetermined *Preamble Text*.
1. Last but not least, we were really not happy with the level of unit testing presented in the original effort. We found the examples to be somewhat contrived and academic in nature, whereas we wanted to present some examples that might prove at least somewhat value-added. We were also successful, we believe, in compiling an end-to-end integration test which, short of subscribing to [*NuGet* packages][NuPkg] themselves, demonstrates that the approach does in fact work.

There are a couple of other nuances.

1. Chiefly concerning our package versioning strategy. Whereas the original work would keep delivered versions aligned, we make no promises concerning such alignment. In the macro, we make an effort for major versions to keep in sync with each other, so you can leverage NuGet version ranges, i.e. ``[1,2)``, however, concerning minor, and especially patch, build, etc, elements, these values can migrate from release to release depending on which packages required an update.

With that said, let us reconsider what it means to Generate Code using Roslyn.

## Overview

Assists in performing Roslyn-based code generation during a build. This includes design-time support, such that code generation can respond to changes made in hand-authored code files by generating new code that shows up to Intellisense as soon as the file is saved to disk.

## Table of Contents

* [How to write your own code generator][]
* [Apply code generation][]
* [Developing your code generator][]
* [Packaging your code generator for others' use][]

## How to write your own code generator
[How to write your own code generator]: #how-to-write-your-own-code-generator

In this walkthrough, we will define a code generator that replicates any class your code generation attribute is applied to, but with a suffix appended to its name.

### Prerequisites

* [.NET Core SDK v2.1+][dotnet-sdk-2.1]. If you do not have v2.1+ there will be cryptic error messages (see [#111](https://github.com/AArnott/CodeGeneration.Roslyn/issues/111)).
* .NET Core SDK v2.1.500 specifically for building this project

[dotnet-sdk-2.1]: https://dotnet.microsoft.com/download/dotnet-core/2.1

### Define code generator
[Define code generator]: #define-code-generator

This must be done in a library that targets `netstandard2.0` or `net461` (or any `netcoreapp2.1`-compatible target). Your generator cannot be defined in the same project that will have code generated for it because code generation runs *prior to* the target project compilation.

Install the [Code.Generation.Roslyn][NuPkg] NuGet Package.

Define the generator class in a class library targeting `netstandard2.0` (*note: constructor accepting `AttributeData` parameter is required*). We have provided a couple of base classes for you, *DocumentCodeGenerator* and *AssemblyCodeGenerator* from which you may derive, depending on your code generation requirements, which ultimately implement either *IDocumentCodeGenerator* or *IAssemblyCodeGenerator*, respectively; however, in this case, we will be focused on *DocumentCodeGenerator*.

```C#
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;

namespace Code.Generation.Roslyn.Generators
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    public class ImplementCloneableInterfaceGenerator : DocumentCodeGenerator
    {
        public ImplementCloneableInterfaceGenerator(AttributeData attributeData) : base(attributeData) { }

        public override Task GenerateAsync(DocumentTransformationContext context, IProgress<Diagnostic> progress, CancellationToken cancellationToken)
        {
            // See the example code for the complete example.
            IEnumerable<CodeGeneratorDescriptor> Generate()
            {
                // ...
            }

            void RunGenerate()
            {
                foreach (var d in Generate())
                {
                    Descriptors.Add(d);
                //  ^^^^^^^^^^^^^^^ This is the key, Descriptors is provided by the base class, your Code
                //                  Generator must simply populate the collection as a result of the Task.
                }
            }

            return Task.Run(RunGenerate, cancellationToken);
        }
    }
}
```

For brevity, feel free to [review the example source](https://github.com/mwpowellhtx/Code.Generation.Roslyn/blob/master/src/Code.Generation.Roslyn.Generators/ImplementCloneableInterfaceGenerator.cs) in our generators test assembly.

### Define attribute

In order to activate your code generator, you need to define an attribute that will ultimately annotate the asset used to trigger the code generation. This attribute may be defined in the same assembly as defines your code generator, but since your code generator must be defined in a `netcoreapp2.1`-compatible library, this may limit which projects can apply your attribute. So define your attribute in another assembly if it must be applied to projects that target older platforms.

If your attributes are in their own project, you must install the [Code.Generation.Roslyn.Attributes][AttrNuPkg] package to your attributes project.

Define your attribute class. For sake of example, we will assume that the attributes are defined in the same netstandard2.0 project that defines the generator which allows us to use the more convenient `typeof` syntax when declaring the code generator type. If the attributes and code generator classes were in separate assemblies, you must specify the assembly-qualified name of the generator type as a string instead.

```C#
using System;
using System.Diagnostics;

namespace Code.Generation.Roslyn.Generators
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    [CodeGenerationAttribute(typeof(ImplementCloneableInterfaceGenerator))]
    [Conditional("CodeGeneration")]
    public class ImplementCloneableInterfaceAttribute : TestAttributeBase
    {
    }
}
```

The ``[Conditional("CodeGeneration")]`` attribute is not necessary, but it will prevent the attribute from persisting in the compiled assembly that consumes it, leaving it instead as just a compile-time hint to code generation, and allowing you to not ship with a dependency on your code generation assembly.

## Apply code generation
[Apply code generation]: #apply-code-generation

The attribute may not be applied in the same assembly that defines the generator. This is because the code generator must be compiled in order to execute before compiling the project that applies the attribute. Also, the consuming project (where the code will be generated) must use SDK-style csproj, which implies using VS2017+ or ``dotnet`` CLI tooling (VS Code with omnisharp, for example).

Applying code generation is incredibly simple. Just add the attribute on any type or member supported by the attribute and generator you wrote. Note you will need to add a project reference to the project that defines the attribute.

```C#
namespace Foo
{
    [Code.Generation.Roslyn.GeneratorsImplementCloneableInterface]
    public class Bar
    {
    }
}
```

Install the [Code.Generation.Roslyn.BuildTime][BuildTimeNuPkg] package into the project that uses your attribute. You may set `PrivateAssets="all"` on this reference because this is a build-time only package. You must also add this item to an `<ItemGroup>` in the project that will execute the code generator as part of your build:

```Xml
<DotNetCliToolReference Include="dotnet-cgr" Version="[1,2)" />
```

You may adjust the version in the above Project Element to match the version of this tool you are using. In this case, we accept the latest [dotnet-cgr][NuPkg] version ranging from *1.0.0.0*, inclusive, up to but not including *2.0.0.0*.

You can then consume the generated code at design-time:

```C#
[Fact]
public void Verify_Cloneable_Interface()
{
    var bar = new Foo.Bar();
    var clone = bar.Clone();
    // And so on...
}
```

You should see Intellisense help you in all your interactions with ``Foo.Bar``.  If you execute [*Go To Definition*][vs-ide-go-to-and-peek-definitions] on it, Visual Studio will open the generated code file that actually defines ``Foo.Bar``, and you will notice it is exactly like ``Bar``, just renamed as our code generator defined it to be.

[vs-ide-go-to-and-peek-definitions]: https://docs.microsoft.com/en-us/visualstudio/ide/go-to-and-peek-definition

### Shared Projects

When using shared projects and partial classes across the definitions of your class in shared and platform projects:

* The code generation attributes should be applied only to the files in the shared project, or, in other words, the attribute should only be applied once per type to avoid multiple generator invocations.
* The ``MSBuild:GenerateCodeFromAttributes`` custom tool must be applied to every file we want to auto generate code from. For convenience, we have provided default Build Properties which you may include:

```Xml
<Import Project="path\to\build\Code.Generation.Roslyn.BuildTime.props" />
```

The ``path\to\...`` will usually be the ``packages\`` path used for your [NuGet restore][nuget-consume-packages-package-restore] process.

[nuget-consume-packages-package-restore]: https://docs.microsoft.com/en-us/nuget/consume-packages/package-restore

## Developing your code generator
[Developing your code generator]: #developing-your-code-generator

Your code generator can be defined in a project in the same solution as the solution with the project that consumes it. You can edit your code generator and build the solution to immediately see the effects of your changes on the generated code.

## Packaging your code generator for others' use
[Packaging your code generator for others' use]: #packaging-your-code-generator-for-others-use

You can also package your code generator as a NuGet package for others to install and use. Your NuGet package should include a dependency on the [``Code.Generation.Roslyn.BuildTime``][BuildTimeNuPkg] that matches the version aligned with the [``Code.Generation.Roslyn``][NuPkg] that you used to produce your generator. We make no promises that the version numbers will be the same, depending entirely on the history of the individual projects. The best advice we can provide is to pay attention to your details, consult with the package pages for the latest versions, their alignment with each other, and so on. The links are all furnished throughout this documentation for your convenience. In this example, for instance, we accept a range from *1.0.0.0*, inclusive, up to but not including *2.0.0.0*.

```Xml
<dependency id="Code.Generation.Roslyn.BuildTime" version="[1,2)" />
```

Your NuGet package should include a ``build`` folder in addition to this dependency, with an *MSBuild* file, either a *.props* or a *.targets* file, that defines an ``GeneratorAssemblySearchPaths`` *MSBuild* item pointing to the folder containing your code generator assembly and its dependencies. For example your package should have a ``build\MyPackage.targets`` file with this content:

```Xml
<?xml version="1.0" encoding="utf-8" ?>
<Project ToolsVersion="14.0" DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <ItemGroup>
    <GeneratorAssemblySearchPaths Include="$(MSBuildThisFileDirectory)..\tools" />
  </ItemGroup>
</Project>
```

Then your package should also have a ``tools`` folder that contains your code generator and any of the runtime dependencies it needs *aside from* those delivered by the [``Code.Generation.Roslyn.BuildTime``][BuildTimeNuPkg] package.

Your attributes assembly should be placed under a ``lib`` folder so consuming projects can apply those attributes.

Your consumers should depend on your package, and the required dotnet CLI tool, so that the *MSBuild Task* can invoke the ``dotnet cgr`` command line tool:

```Xml
<ItemGroup>
  <PackageReference Include="YourCodeGenPackage" Version="1.2.3" PrivateAssets="all" />
  <DotNetCliToolReference Include="dotnet-cgr" Version="[1,2)" />
</ItemGroup>
```

Again, in this example we allow for an acceptable version range in the CLI dependency.

[NuPkg]: https://nuget.org/packages/Code.Generation.Roslyn
[EngineNuPkg]: https://nuget.org/packages/Code.Generation.Roslyn.Engine
[AttrNuPkg]: https://nuget.org/packages/Code.Generation.Roslyn.Attributes
[BuildTimeNuPkg]: https://nuget.org/packages/Code.Generation.Roslyn.BuildTime
[ToolNuPkg]: https://nuget.org/packages/dotnet-cgr
[netstandard-table]: https://docs.microsoft.com/dotnet/standard/net-standard#net-implementation-support
