using Cocona;
using Cocona.CommandLine;

using Microsoft.Extensions.DependencyInjection;

using Spectre.Console;
using Spectre.Console.Testing;

namespace SquiggleCop.Tool.Tests;

[Collection("NoParallelization")] // App host sets `Environment.ExitCode`, which isn't thread-safe
public class CommandTests : TestBase
{
    private readonly TestConsole _console;

    public CommandTests()
    {
        _console = new();
        _console.Profile.Width = 300; // Set a wide console width to avoid wrapping in baselines
    }

    [Theory]
    [CombinatorialData]
    public async Task GenerateCommand(bool autoBaseline, FileTypes fileTypes)
    {
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

        CoconaApp app = App.Create(builder =>
        {
            builder.Services.AddSingleton<IAnsiConsole>(_console);
            builder.Services.AddSingleton<ICoconaEnvironmentProvider>(new TestCoconaEnvironmentProvider(args));
        });

        await app.RunAsync();

        CommandResults results = new(_console.Lines, Environment.ExitCode);

        await Verify(results)
            .ScrubDirectory(TestRootPath, "{TestRootPath}")
            .ScrubPathSeparators()
            .UseParameters(autoBaseline, fileTypes);
    }

    [Theory]
    [CombinatorialData]
    public async Task GenerateCommandDiff(bool autoBaseline)
    {
        await CopyTestData("SquiggleCop.v2.Baseline.yaml");
        await CopyTestData("Log.v2.sarif");

        // Ensure there's a diff between the baseline and the SARIF log
        string[] baseline = await File.ReadAllLinesAsync(Path.Combine(TestRootPath, "SquiggleCop.v2.Baseline.yaml"));
        baseline = baseline.Where(line => !line.Contains("Id: CA1000", StringComparison.Ordinal) && !line.Contains("Id: IDE0004", StringComparison.Ordinal)).ToArray();
        await File.WriteAllLinesAsync(Path.Combine(TestRootPath, "SquiggleCop.v2.Baseline.yaml"), baseline);

        string[] args = $"generate {Path.Combine(TestRootPath, "Log.v2.sarif")} --output {Path.Combine(TestRootPath, "SquiggleCop.v2.Baseline.yaml")}".Split(" ");

        if (autoBaseline)
        {
            args = [.. args, "--auto-baseline"];
        }

        CoconaApp app = App.Create(builder =>
        {
            builder.Services.AddSingleton<IAnsiConsole>(_console);
            builder.Services.AddSingleton<ICoconaEnvironmentProvider>(new TestCoconaEnvironmentProvider(args));
        });

        await app.RunAsync();

        CommandResults results = new(_console.Lines, Environment.ExitCode);

        await Verify(results)
            .ScrubDirectory(TestRootPath, "{TestRootPath}")
            .ScrubPathSeparators()
            .UseParameters(autoBaseline);
    }
}
