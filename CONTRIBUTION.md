## Contribution Guidelines

### Recasting Code.Generation.Roslyn

We have taken the opportunity to recast the original *CodeGeneration.Roslyn* <sup>[[1]]</sup> in several primary ways.

1. Re-focus Code Generation on just that: ***FACILITATING CODE GENERATION***, specifically in the form of a series of *CompilationUnitSyntax* <sup>[[2]]</sup>.
1. To be ultra clear about this, *Code.Generation.Roslyn* <sup>[[3]]</sup> (note the difference in punctuation), is not in the business of second guessing anything about your code generators. This includes such things as whether there were namespaces involved, classes, structs, or whatever.
1. Registries of required Assembly references are maintained during the Code Generation process. The Assembly references is nothing new, but the manner in which we keep track of the Registry is refined.
1. More critically, we try to prohibit code from generating when triggering assets, the Assembly references, but more importantly the source files themselves in which your Code Generation Attributes appeared as annotations.
1. We also aggressively purge previously generated code when triggering source files were renamed, moved, or removed.
1. A big difference between our offering and the original work is in what it is how code generators respond. We want a descriptor in response, which includes a collection of CompilationUnitSyntax items, not MemberDeclarationSyntax. Yes, we want you to produce unit level code.
1. Another difference is that we also allow you to furnish your own boilerplate preamble comments. If you decline to do this, we will default to canned preamble comments. However, if you do choose to do this, the responsibility rests on your authorship to ensure the comments are correctly formatted as such.
1. Last, but certainly not least, the responsibility for generating appropriate code rests entirely on your *ICodeGenerator* <sup>[[4]]</sup> authorship. We have included a couple of generator examples, which we use throughout the unit tests, as examples for you, along these very lines.

### Changes in the unit testing strategy

We were able to improve upon the unit testing strategy in big, big ways. Short of actually invoking the Code Generation Tooling on the command line, let along via Microsoft Build Targets, we were very successfully at invoking the Tooling code programmaticcally.

Therefore, unlike with its predecessor, there is no need to unload any projects as you verify or consider enhancing the unit tests. We have made an effort to capture a handful of likely, even a handful of compelling, certainly more interesting than academic, Code Generation scenarios based on a canned set of project assets.

### Requirements

As far as we know, Windows 10 requirements still hold. We have not addressed anything along these lines, so we do not expect the issue here to be any different.

* If you are running on Windows 10, you need to *install the .NET 3.5 optional feature* <sup>[[5]]</sup> in order to compile the project. However, after reading the blogs, this *may be easier said than done* <sup>[[6, 7]]</sup>.

<sup>[[1]]</sup> [CodeGeneration.Roslyn Github Repository](https://github.com/aarnott/CodeGeneration.Roslyn)
<sup>[[2]]</sup> [*Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax*](https://docs.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.csharp.syntax.compilationunitsyntax)
<sup>[[3]]</sup> [Code.Generation.Roslyn Github Repository](https://github.com/mwpowellhtx/Code.Generation.Roslyn)
<sup>[[4]]</sup> [*Code.Generation.Roslyn.ICodeGenerator*](https://github.com/mwpowellhtx/Code.Generation.Roslyn/blob/master/src/Code.Generation.Roslyn/ICodeGenerator.cs)
<sup>[[5]]</sup> [Install the .NET Framework 3.5 on Windows 10, Windows 8.1, and Windows 8](https://docs.microsoft.com/en-us/dotnet/framework/install/dotnet-35-windows-10)
<sup>[[6]]</sup> [Windows 10 Will not install .Net 3.5 Framework](https://answers.microsoft.com/en-us/windows/forum/windows_10-windows_install/windows-10-will-not-install-net-35-framework/8f07cbcc-74f1-480c-8c0f-35a18056c5f9)
<sup>[[7]]</sup> [Adding features (including .NET 3.5) to Windows 10](https://blogs.technet.microsoft.com/mniehaus/2015/08/31/adding-features-including-net-3-5-to-windows-10/)
