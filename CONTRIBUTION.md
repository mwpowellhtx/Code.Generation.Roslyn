## Contribution Guidelines

### Build failures due to locked files

When the solution is open in Visual Studio, the *Code.Generation.Roslyn.Tasks* <sup>[[1]]</sup> project build may fail due to its output assembly being locked on disk. This is because the test project in the solution consumes this DLL as part of its design build and VS does not unload the file.

There are two workarounds for this:

1. Unload the *Code.Generation.Roslyn.Tasks* <sup>[[1]]</sup> project. Once it is built once, you do not tend to need it to build again anyway. Most code changes are not to that assembly.
1. Unload the *Code.Generation.Roslyn.Tests* <sup>[[2]]</sup> project and restart Visual Studio. This will prevent the design time builds in Visual Studio from loading and locking the output of the *Code.Generation.Roslyn.Tasks* [[1]] project, allowing you to make changes to that project and build them.

### Requirements

If you are running on Windows 10, you need to [install the .NET 3.5 optional feature](https://docs.microsoft.com/en-us/dotnet/framework/install/dotnet-35-windows-10) <sup>[[3]]</sup> in order to compile the project. Reading the blogs, this *may be easier said than done* <sup>[[4]]</sup>.

<sup>[[1]]</sup> [*Code.Generation.Roslyn.Tasks*](/mwpowellhtx/Code.Generation.Roslyn/tree/master/src/Code.Generation.Roslyn.Tasks)
<sup>[[2]]</sup> [*Code.Generation.Roslyn.Tests*](/mwpowellhtx/Code.Generation.Roslyn/tree/master/src/Code.Generation.Roslyn.Tests)
<sup>[[3]]</sup> [Adding features (including .NET 3.5) to Windows 10](https://blogs.technet.microsoft.com/mniehaus/2015/08/31/adding-features-including-net-3-5-to-windows-10/)
<sup>[[4]]</sup> [Windows 10 Will not install .Net 3.5 Framework](https://answers.microsoft.com/en-us/windows/forum/windows_10-windows_install/windows-10-will-not-install-net-35-framework/8f07cbcc-74f1-480c-8c0f-35a18056c5f9)
