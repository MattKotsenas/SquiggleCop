using System.Reflection;

using Cocona.Application;

namespace SquiggleCop.Tool;

internal class DotnetToolApplicationMetadataProvider : ICoconaApplicationMetadataProvider
{
    public string GetDescription()
    {
        return "SquiggleCop CLI - Generate and review analyzer configuration";
    }

    public string GetExecutableName()
    {
        return "dotnet squigglecop";
    }

    public string GetProductName()
    {
        return "SquiggleCop CLI";
    }

    public string GetVersion()
    {
        var entryAssembly = Assembly.GetEntryAssembly();

        return entryAssembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
               ?? entryAssembly?.GetCustomAttribute<AssemblyVersionAttribute>()?.Version
               ?? "1.0.0.0";
    }
}
