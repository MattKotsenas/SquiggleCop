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
        const string errorLog = "sarif.log";

        ProjectCreator.Templates.SimpleBuild()
            .PropertyGroup()
                .ErrorLog(errorLog, "2.1")
                .AutoBaseline(autoBaseline)
            .Target(name: "_SetSarifLog", beforeTargets: "AfterCompile")
                .TaskMessage("Overwriting ErrorLog with sample to simulate compile...")
                .CopyFileTask(Path.Combine(TestRootPath, "sample1.log"), errorLog)
            .Save(Path.Combine(TestRootPath, "project.csproj"))
            .TryBuild(restore: true, out bool result, out BuildOutput output);

        await Verify(
            new BaselineFileResults(
                output.ToBuildLogMessages(),
                await baselineFile.ReadAllTextAsyncOrDefault()
        )).UseParameters(autoBaseline, explicitFile);
        result.Should().BeTrue();

        if (autoBaseline ?? false)
        {
            baselineFile.Exists.Should().BeTrue();
            baselineFile.LastWriteTimeUtc.Should().BeOnOrAfter(now);
        }
        else
        {
            baselineFile.Exists.Should().BeFalse();
        }
    }

    [Theory]
    [CombinatorialData]
    public async Task BaselineUpToDate(bool? autoBaseline, bool explicitFile)
    {
        DateTime now = DateTime.UtcNow;
        FileInfo baselineFile = new(Path.Combine(TestRootPath, SquiggleCop.BaselineFile));
        const string errorLog = "sarif.log";

        ProjectCreator.Templates.SimpleBuild()
            .PropertyGroup()
                .ErrorLog(errorLog, "2.1")
                .AutoBaseline(autoBaseline)
            .Target(name: "_SetSarifLog", beforeTargets: "AfterCompile")
                .TaskMessage("Overwriting ErrorLog with sample to simulate compile...")
                .CopyFileTask(Path.Combine(TestRootPath, "sample1.log"), errorLog)
                .TaskMessage("Write sample to simulate up-to-date baseline...")
                .CopyFileTask(Path.Combine(TestRootPath, "sample1.baseline"), baselineFile.FullName)
                .TouchFilesTask([baselineFile.FullName], lastWriteTime: now.AddDays(-1))
            .Save(Path.Combine(TestRootPath, "project.csproj"))
            .TryBuild(restore: true, out bool result, out BuildOutput output);

        await Verify(
            new BaselineFileResults(
                output.ToBuildLogMessages(),
                await baselineFile.ReadAllTextAsyncOrDefault()
        )).UseParameters(autoBaseline, explicitFile);
        result.Should().BeTrue();
        baselineFile.Exists.Should().BeTrue();
        baselineFile.LastWriteTimeUtc.Should().BeBefore(now);
    }

    [Theory]
    [CombinatorialData]
    public async Task BaselineOutOfDate(bool? autoBaseline, bool explicitFile)
    {
        DateTime now = DateTime.UtcNow;
        FileInfo baselineFile = new(Path.Combine(TestRootPath, SquiggleCop.BaselineFile));
        const string errorLog = "sarif.log";

        ProjectCreator.Templates.SimpleBuild()
            .PropertyGroup()
                .ErrorLog(errorLog, "2.1")
                .AutoBaseline(autoBaseline)
            .Target(name: "_SetSarifLog", beforeTargets: "AfterCompile")
                .TaskMessage("Overwriting ErrorLog with sample to simulate compile...")
                .CopyFileTask(Path.Combine(TestRootPath, "sample1.log"), errorLog)
                .TaskMessage("Write sample to simulate out-of-date baseline...")
                .TouchFilesTask([baselineFile.FullName], lastWriteTime: now.AddDays(-1))
            .Save(Path.Combine(TestRootPath, "project.csproj"))
            .TryBuild(restore: true, out bool result, out BuildOutput output);

        await Verify(
            new BaselineFileResults(
                output.ToBuildLogMessages(),
                await baselineFile.ReadAllTextAsyncOrDefault()
        )).UseParameters(autoBaseline, explicitFile);
        result.Should().BeTrue();
        baselineFile.Exists.Should().BeTrue();

        if (autoBaseline ?? false)
        {
            baselineFile.LastWriteTimeUtc.Should().BeOnOrAfter(now);
        }
        else
        {
            baselineFile.LastWriteTimeUtc.Should().BeBefore(now);
        }
    }
}
