using System.Runtime.CompilerServices;

using Microsoft.Build.Utilities.ProjectCreation;

namespace SquiggleCop.Tasks.Tests;

internal static class ModuleInitializer
{
    [ModuleInitializer]
    internal static void InitializeMSBuild()
    {
        MSBuildAssemblyResolver.Register();
    }

    [ModuleInitializer]
    public static void InitializeVerify()
    {
        VerifyNupkg.Initialize();
    }
}
