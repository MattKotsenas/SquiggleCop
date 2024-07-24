using Cocona.CommandLine;

namespace SquiggleCop.Tool.Tests;

internal class TestCoconaEnvironmentProvider : ICoconaEnvironmentProvider
{
    private readonly string[] _args;

    public TestCoconaEnvironmentProvider(string[] args)
    {
        _args = args;
    }

    public string[] GetCommandLineArgs() => _args;
}
