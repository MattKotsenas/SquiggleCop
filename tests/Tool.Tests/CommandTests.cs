using Cocona;
using Cocona.CommandLine;

using Microsoft.Extensions.DependencyInjection;

using Spectre.Console;
using Spectre.Console.Testing;

namespace SquiggleCop.Tool.Tests;

[Collection("NoParallelization")] // App host sets `Environment.ExitCode`, which isn't thread-safe
public class CommandTests : TestBase
{
    [Theory]
    [CombinatorialData]
    public async Task GenerateCommand(bool autoBaseline, FileTypes fileTypes)
    {
        TestConsole console = new();
        console.Profile.Width = 300; // Set a wide console width to avoid wrapping in baselines

        await CopyTestData("Log.v1.sarif");
        await CopyTestData("Log.v2.sarif");

        string file = fileTypes switch
        {
            FileTypes.NotFound => "NotFound.sarif",
            FileTypes.Valid => "Log.v2.sarif",
            FileTypes.Invalid => "Log.v1.sarif",
            _ => throw new ArgumentOutOfRangeException(nameof(fileTypes))
        };

        string[] args = $"generate {Path.Combine(TestRootPath, file)} --output {Path.Combine(TestRootPath, "output")}".Split(" ");

        if (autoBaseline)
        {
            args = [.. args, "--auto-baseline"];
        }

        CoconaApp app = AppBuilder.Create(builder =>
        {
            builder.Services.AddSingleton<IAnsiConsole>(console);
            builder.Services.AddSingleton<ICoconaEnvironmentProvider>(new TestCoconaEnvironmentProvider(args));
        });

        await app.RunAsync();

        CommandResults results = new(console.Lines, Environment.ExitCode);

        await Verify(results)
            .ScrubDirectory(TestRootPath, "{TestRootPath}")
            .ScrubPathSeparators()
            .UseParameters(autoBaseline, fileTypes);
    }
}
