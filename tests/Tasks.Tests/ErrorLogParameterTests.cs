using Microsoft.Build.Utilities.ProjectCreation;

namespace SquiggleCop.Tasks.Tests;

public class ErrorLogParameterTests : TestBase
{
    [Fact]
    public async Task NoErrorLogReportsWarning()
    {
        ProjectCreator.Templates.SimpleBuild()
            .Save(Path.Combine(TestRootPath, "project.csproj"))
            .TryBuild(restore: true, out bool result, out BuildOutput output);

        result.Should().BeTrue();
        await Verify(output.ToBuildLogMessages());
    }

    [Fact]
    public async Task MissingErrorLogReportsWarning()
    {
        const string sarifFileName = "sarif.log";

        ProjectCreator.Templates.SimpleBuild()
            .PropertyGroup()
                .ErrorLog(sarifFileName, "2.1")
            .Target(name: "_DeleteSarifLogBeforeSquiggleCopRuns", beforeTargets: "AfterCompile")
                .TaskMessage("Deleting ErrorLog...")
                .Task(name: "Delete", parameters: new Dictionary<string, string?>(StringComparer.Ordinal) { { "Files", sarifFileName } })
            .Save(Path.Combine(TestRootPath, "project.csproj"))
            .TryBuild(restore: true, out bool result, out BuildOutput output);

        result.Should().BeTrue();
        await Verify(output.ToBuildLogMessages());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("1")]
    public async Task V1ErrorLogReportsWarning(string? version)
    {
        ProjectCreator.Templates.SimpleBuild()
            .PropertyGroup()
                .ErrorLog("sarif.log", version)
            .Save(Path.Combine(TestRootPath, "project.csproj"))
            .TryBuild(restore: true, out bool result, out BuildOutput output);

        result.Should().BeTrue();
        await Verify(output.ToBuildLogMessages())
            .UseParameters(version);
    }

    [Theory]
    [InlineData("2")]
    [InlineData("2.1")]
    public async Task V2ErrorLogNoWarning(string version)
    {
        ProjectCreator.Templates.SimpleBuild()
            .PropertyGroup()
                .ErrorLog("sarif.log", version)
                .AutoBaseline(true)
            .Save(Path.Combine(TestRootPath, "project.csproj"))
            .TryBuild(restore: true, out bool result, out BuildOutput output);

        result.Should().BeTrue();
        await Verify(output.ToBuildLogMessages())
            .UseParameters(version);
    }
}
