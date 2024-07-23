using CliWrap;
using CliWrap.Buffered;

namespace Tool.Tests;


public class CommandLineTests : TestBase
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

        await Verify(result).IgnoreMembers(typeof(CommandResult), "StartTime", "ExitTime", "RunTime");
    }
}
