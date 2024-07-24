using Microsoft.Build.Utilities.ProjectCreation;

namespace SquiggleCop.Tasks.Tests;

public class BaselineFileTests : TestBase
{
    private DateTime Now { get; } = DateTime.UtcNow;
    private string ErrorLog { get; } = "sarif.log";

    private FileInfo GetBaselineFile(bool explicitFile) =>
        new(Path.Combine(TestRootPath, explicitFile ? "explicitFileSubdirectory" : "", "SquiggleCop.Baseline.yaml"));

    // TODO: Add `\r\n` and `\n` in baseline files to test matrix

    [Theory]
    [CombinatorialData]
    public async Task NoBaselineFile(bool enabled, bool? autoBaseline, bool explicitFile)
    {
        FileInfo baselineFile = GetBaselineFile(explicitFile);

        ProjectCreator project = ProjectCreator.Templates.SimpleBuild()
            .PropertyGroup()
                .ErrorLog(ErrorLog, "2.1")
                .AutoBaseline(autoBaseline)
                .Enabled(enabled)
            .Target(name: "_SetSarifLog", beforeTargets: "AfterCompile")
                .TaskMessage("Overwriting ErrorLog with sample to simulate compile...")
                .WriteLinesToFileTask(ErrorLog, TestData.Sample1.Sarif);

        if (explicitFile)
        {
            project.ItemGroup().AdditionalFiles(baselineFile.FullName);
        }

        project
            .Save(Path.Combine(TestRootPath, "project.csproj"))
            .TryBuild(restore: true, out bool result, out BuildOutput output);

        await Verify(
            new BaselineFileResults(
                result,
                output.ToBuildLogMessages(),
                baselineFile.Exists,
                baselineFile.WasWritten(Now)))
            .UseParameters(enabled, autoBaseline, explicitFile)
            .ScrubDirectory(TestRootPath, "{TestRootPath}")
            .ScrubPathSeparators();
    }

    [Theory]
    [CombinatorialData]
    public async Task BaselineUpToDate(bool enabled, bool? autoBaseline, bool explicitFile, bool shouldIncrementalBuild)
    {
        FileInfo baselineFile = GetBaselineFile(explicitFile);
        DateTime errorLogWriteTime = shouldIncrementalBuild ? Now.AddDays(-1) : Now.AddDays(1);

        ProjectCreator project = ProjectCreator.Templates.SimpleBuild()
            .PropertyGroup()
                .ErrorLog(ErrorLog, "2.1")
                .AutoBaseline(autoBaseline)
                .Enabled(enabled)
            .Target(name: "_SetSarifLog", beforeTargets: "AfterCompile")
                .TaskMessage("Overwriting ErrorLog with sample to simulate compile...")
                .WriteLinesToFileTask(ErrorLog, TestData.Sample1.Sarif)
                .TouchFilesTask([ErrorLog], lastWriteTime: errorLogWriteTime)
                .TaskMessage("Write sample to simulate up-to-date baseline...")
                .WriteLinesToFileTask(baselineFile.FullName, TestData.Sample1.Baseline)
                .TouchFilesTask([baselineFile.FullName], lastWriteTime: errorLogWriteTime.AddSeconds(1));

        if (explicitFile)
        {
            project.ItemGroup().AdditionalFiles(baselineFile.FullName);
        }

        project
            .Save(Path.Combine(TestRootPath, "project.csproj"))
            .TryBuild(restore: true, out bool result, out BuildOutput output);

        await Verify(
            new BaselineFileResults(
                result,
                output.ToBuildLogMessages(),
                baselineFile.Exists,
                baselineFile.WasWritten(Now)))
            .UseParameters(enabled, autoBaseline, explicitFile, shouldIncrementalBuild)
            .ScrubDirectory(TestRootPath, "{TestRootPath}")
            .ScrubPathSeparators();
    }

    [Theory]
    [CombinatorialData]
    public async Task BaselineOutOfDate(bool enabled, bool? autoBaseline, bool explicitFile, bool shouldIncrementalBuild)
    {
        FileInfo baselineFile = GetBaselineFile(explicitFile);
        DateTime errorLogWriteTime = shouldIncrementalBuild ? Now.AddDays(-1) : Now.AddDays(1);

        ProjectCreator project = ProjectCreator.Templates.SimpleBuild()
            .PropertyGroup()
                .ErrorLog(ErrorLog, "2.1")
                .AutoBaseline(autoBaseline)
                .Enabled(enabled)
            .Target(name: "_SetSarifLog", beforeTargets: "AfterCompile")
                .TaskMessage("Overwriting ErrorLog with sample to simulate compile...")
                .WriteLinesToFileTask(ErrorLog, TestData.Sample1.Sarif)
                .TouchFilesTask([ErrorLog], lastWriteTime: errorLogWriteTime)
                .TaskMessage("Write sample to simulate out-of-date baseline...")
                .MakeDirTask([baselineFile.DirectoryName!])
                .TouchFilesTask([baselineFile.FullName], lastWriteTime: errorLogWriteTime.AddSeconds(1));

        if (explicitFile)
        {
            project.ItemGroup().AdditionalFiles(baselineFile.FullName);
        }

        project
            .Save(Path.Combine(TestRootPath, "project.csproj"))
            .TryBuild(restore: true, out bool result, out BuildOutput output);

        await Verify(
            new BaselineFileResults(
                result,
                output.ToBuildLogMessages(),
                baselineFile.Exists,
                baselineFile.WasWritten(Now)))
            .UseParameters(enabled, autoBaseline, explicitFile, shouldIncrementalBuild)
            .ScrubDirectory(TestRootPath, "{TestRootPath}")
            .ScrubPathSeparators();
    }
}
