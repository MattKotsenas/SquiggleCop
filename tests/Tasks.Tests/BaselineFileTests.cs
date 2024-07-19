using System.Reflection.Metadata;

using Microsoft.Build.Utilities.ProjectCreation;

namespace SquiggleCop.Tasks.Tests;

public class BaselineFileTests : TestBase
{
    public BaselineFileTests()
    {
        // Create sample SARIF and baseline files.
        // These are considered "well known file names" inside the tests.
        File.WriteAllText(Path.Combine(TestRootPath, "sample1.log"), TestData.Sample1.Sarif);
        File.WriteAllText(Path.Combine(TestRootPath, "sample1.baseline"), TestData.Sample1.Baseline);
        File.WriteAllText(Path.Combine(TestRootPath, "sample1.log"), TestData.Sample1.Sarif);
        File.WriteAllText(Path.Combine(TestRootPath, "sample2.baseline"), TestData.Sample2.Baseline);
    }

    // TODO: Implement explicit file support
    // TODO: Add `\r\n` and `\n` in baseline files to test matrix
    // TODO: Test incremental build

    [Theory]
    [CombinatorialData]
    public async Task NoBaselineFile(bool? autoBaseline, bool explicitFile)
    {
        DateTime now = DateTime.UtcNow;
        FileInfo baselineFile = new(Path.Combine(TestRootPath, SquiggleCop.BaselineFile));

        if (autoBaseline.HasValue && !autoBaseline.Value)
        {
            // TODO: Implement; when auto is off, do what? Give message to run tool?
            return;
        }

        const string errorLog = "sarif.log";

        ProjectCreator.Templates.SimpleBuild()
            .PropertyGroup()
                .ErrorLog(errorLog, "2.1")
                .Property("SquiggleCop_AutoBaseline", autoBaseline?.ToString().ToLowerInvariant())
            .Target(name: "_SetSarifLog", beforeTargets: "AfterCompile")
                .TaskMessage("Overwriting ErrorLog with sample to simulate compile...")
                .Task(name: "Copy", parameters: new Dictionary<string, string?>(StringComparer.Ordinal) { { "SourceFiles", Path.Combine(TestRootPath, "sample1.log") }, { "DestinationFiles", errorLog } })
            .Save(Path.Combine(TestRootPath, "project.csproj"))
            .TryBuild(restore: true, out bool result, out BuildOutput output);

        result.Should().BeTrue();
        baselineFile.LastWriteTimeUtc.Should().BeOnOrAfter(now);
        await Verify(
            new BaselineFileResults(
                output.ToBuildLogMessages(),
                await File.ReadAllTextAsync(baselineFile.FullName)
        )).UseParameters(autoBaseline, explicitFile);
    }

    [Theory]
    [CombinatorialData]
    public async Task BaselineUpToDate(bool? autoBaseline, bool explicitFile)
    {
        DateTime now = DateTime.UtcNow;
        FileInfo baselineFile = new(Path.Combine(TestRootPath, SquiggleCop.BaselineFile));

        if (autoBaseline.HasValue && !autoBaseline.Value)
        {
            // TODO: Implement; when auto is off, do what? Give message to run tool?
            return;
        }

        const string errorLog = "sarif.log";

        ProjectCreator.Templates.SimpleBuild()
            .PropertyGroup()
                .ErrorLog(errorLog, "2.1")
                .Property("SquiggleCop_AutoBaseline", autoBaseline?.ToString().ToLowerInvariant())
            .UsingRoslynCodeTask("_SetBaselineLastWriteTime",
                $"""
                // Set the last write time to a time in the past to simulate the file being up-to-date
                File.SetLastWriteTimeUtc(@"{baselineFile.FullName}", DateTime.UtcNow.AddDays(-1));
                """)
            .Target(name: "_SetSarifLog", beforeTargets: "AfterCompile")
                .TaskMessage("Overwriting ErrorLog with sample to simulate compile...")
                .Task(name: "Copy", parameters: new Dictionary<string, string?>(StringComparer.Ordinal) { { "SourceFiles", Path.Combine(TestRootPath, "sample1.log") }, { "DestinationFiles", errorLog } })
                .TaskMessage("Write sample to simulate up-to-date baseline...")
                .Task(name: "Copy", parameters: new Dictionary<string, string?>(StringComparer.Ordinal) { { "SourceFiles", Path.Combine(TestRootPath, "sample1.baseline") }, { "DestinationFiles", baselineFile.FullName } })
                .Task("_SetBaselineLastWriteTime")
            .Save(Path.Combine(TestRootPath, "project.csproj"))
            .TryBuild(restore: true, out bool result, out BuildOutput output);

        result.Should().BeTrue();
        baselineFile.LastWriteTimeUtc.Should().BeBefore(now);
        await Verify(
            new BaselineFileResults(
                output.ToBuildLogMessages(),
                await File.ReadAllTextAsync(baselineFile.FullName)
        )).UseParameters(autoBaseline, explicitFile);
    }

    [Theory]
    [CombinatorialData]
    public void BaselineOutOfDate(bool? autoBaseline, bool explicitFile)
    {
        // TODO: Implement; when auto is on, update the file
        // TODO: Implement; when auto is off, pop Verify? Give message to run tool?
    }
}
