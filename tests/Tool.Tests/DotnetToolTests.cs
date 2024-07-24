using CliWrap;
using CliWrap.Buffered;

namespace SquiggleCop.Tool.Tests;

/// <summary>
/// Snapshot tests of the help commands. Verifies:
///   - That the package can be installed and run
///   - Required and optional arguments
/// </summary>
[Collection("NoParallelization")] // Running multiple instances of dotnet tools in parallel is not thread-safe
public class DotnetToolTests : TestBase
{
    private async Task<BufferedCommandResult> Install(string temp, string nugetConfig)
    {
        // Add our temp directory to %PATH% so installed tools can be found and executed
        Environment.SetEnvironmentVariable("PATH", Environment.GetEnvironmentVariable("PATH") + $"{Path.PathSeparator}{TestRootPath}");

        string[] args = $"tool install squigglecop.tool --tool-path {temp} --prerelease --configfile {nugetConfig}".Split(" ");

        return await Cli.Wrap("dotnet")
        .WithArguments(args)
        .ExecuteBufferedAsync()
        .ConfigureAwait(false);
    }

    [Fact]
    public async Task PrintUsage()
    {
        await Install(TestRootPath, Repository.NuGetConfigPath);

        BufferedCommandResult result = await Cli.Wrap("dotnet-squigglecop").ExecuteBufferedAsync();

        await Verify(result).IgnoreCommandResultTimes();
    }

    [Fact]
    public async Task PrintGenerateUsage()
    {
        await Install(TestRootPath, Repository.NuGetConfigPath);

        BufferedCommandResult result = await Cli.Wrap("dotnet-squigglecop")
            .WithArguments("generate --help".Split(" "))
            .WithValidation(CommandResultValidation.None)
            .ExecuteBufferedAsync();

        await Verify(result).IgnoreCommandResultTimes();
    }
}
