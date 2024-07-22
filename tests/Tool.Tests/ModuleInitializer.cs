using System.Runtime.CompilerServices;

namespace SquiggleCop.Tool.Tests;

internal static class ModuleInitializer
{
    [ModuleInitializer]
    public static void InitializeVerify()
    {
        VerifyNupkg.Initialize();
    }
}
