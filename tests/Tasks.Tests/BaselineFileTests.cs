using Microsoft.Build.Utilities.ProjectCreation;

namespace SquiggleCop.Tasks.Tests;

public class BaselineFileTests : TestBase
{
    private FileInfo GetBaselineFile(bool explicitFile) =>
        new(Path.Combine(TestRootPath, explicitFile ? "explicitFileSubdirectory" : "", "SquiggleCop.Baseline.yaml"));

    // TODO: Add `\r\n` and `\n` in baseline files to test matrix

    [Theory]
    [CombinatorialData]
    public async Task NoBaselineFile(bool? autoBaseline, bool explicitFile)
    {
        DateTime now = DateTime.UtcNow;
        FileInfo baselineFile = GetBaselineFile(explicitFile);
        const string errorLog = "sarif.log";

        ProjectCreator project = ProjectCreator.Templates.SimpleBuild()
            .PropertyGroup()
                .ErrorLog(errorLog, "2.1")
                .AutoBaseline(autoBaseline)
            .Target(name: "_SetSarifLog", beforeTargets: "AfterCompile")
                .TaskMessage("Overwriting ErrorLog with sample to simulate compile...")
                .WriteLinesToFileTask(errorLog, TestData.Sample1.Sarif);

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
                baselineFile.WasWritten(now)))
            .UseParameters(autoBaseline, explicitFile)
            .ScrubDirectory(TestRootPath, "{TestRootPath}")
            .ScrubPathSeparators();
    }

    [Theory]
    [CombinatorialData]
    public async Task BaselineUpToDate(bool? autoBaseline, bool explicitFile, bool shouldIncrementalBuild)
    {
        DateTime now = DateTime.UtcNow;
        FileInfo baselineFile = GetBaselineFile(explicitFile);
        const string errorLog = "sarif.log";
        DateTime errorLogWriteTime = shouldIncrementalBuild ? now.AddDays(-1) : now.AddDays(1);

        ProjectCreator project = ProjectCreator.Templates.SimpleBuild()
            .PropertyGroup()
                .ErrorLog(errorLog, "2.1")
                .AutoBaseline(autoBaseline)
            .Target(name: "_SetSarifLog", beforeTargets: "AfterCompile")
                .TaskMessage("Overwriting ErrorLog with sample to simulate compile...")
                .WriteLinesToFileTask(errorLog, TestData.Sample1.Sarif)
                .TouchFilesTask([errorLog], lastWriteTime: errorLogWriteTime)
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
                baselineFile.WasWritten(now)))
            .UseParameters(autoBaseline, explicitFile, shouldIncrementalBuild)
            .ScrubDirectory(TestRootPath, "{TestRootPath}")
            .ScrubPathSeparators();
    }

    [Theory]
    [CombinatorialData]
    public async Task BaselineOutOfDate(bool? autoBaseline, bool explicitFile, bool shouldIncrementalBuild)
    {
        DateTime now = DateTime.UtcNow;
        FileInfo baselineFile = GetBaselineFile(explicitFile);
        const string errorLog = "sarif.log";
        DateTime errorLogWriteTime = shouldIncrementalBuild ? now.AddDays(-1) : now.AddDays(1);

        ProjectCreator project = ProjectCreator.Templates.SimpleBuild()
            .PropertyGroup()
                .ErrorLog(errorLog, "2.1")
                .AutoBaseline(autoBaseline)
            .Target(name: "_SetSarifLog", beforeTargets: "AfterCompile")
                .TaskMessage("Overwriting ErrorLog with sample to simulate compile...")
                .WriteLinesToFileTask(errorLog, TestData.Sample1.Sarif)
                .TouchFilesTask([errorLog], lastWriteTime: errorLogWriteTime)
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
                baselineFile.WasWritten(now)))
            .UseParameters(autoBaseline, explicitFile, shouldIncrementalBuild)
            .ScrubDirectory(TestRootPath, "{TestRootPath}")
            .ScrubPathSeparators();
    }
}
