﻿using System.Runtime.CompilerServices;

using Microsoft.Build.Utilities.ProjectCreation;

namespace SquiggleCop.Common.Tests;

internal static class ModuleInitializer
{
    [ModuleInitializer]
    internal static void InitializeMSBuild()
    {
        MSBuildAssemblyResolver.Register();
    }
}