using Microsoft.Build.Utilities.ProjectCreation;

namespace SquiggleCop.Common.Tests;

public class ErrorLogParameterTests : TestBase
{
    [Fact]
    public async Task NoErrorLogReportsWarning()
    {
        ProjectCreator.Templates.SimpleBuild()
            .Save(Path.Combine(TestRootPath, "project.csproj"))
            .TryBuild(restore: true, out bool result, out BuildOutput output);

        result.Should().BeTrue();
        await Verify(output.WarningEvents.ToBuildLogMessages());
    }

    [Fact]
    public async Task MissingErrorLogReportsWarning()
    {
        const string sarifFileName = "sarif.log";

        ProjectCreator.Templates.SimpleBuild()
            .PropertyGroup()
                .Property("ErrorLog", $"{sarifFileName},version=2.1")
            .Target(name: "_DeleteSarifLogBeforeSquiggleCopRuns", beforeTargets: "AfterCompile")
                .TaskMessage("Deleting ErrorLog.")
                .Task(name: "Delete", parameters: new Dictionary<string, string?>(StringComparer.Ordinal) { { "Files", sarifFileName } })
            .Save(Path.Combine(TestRootPath, "project.csproj"))
            .TryBuild(restore: true, out bool result, out BuildOutput output);

        result.Should().BeTrue();
        await Verify(output.WarningEvents.ToBuildLogMessages());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("1")]
    public void V1ErrorLogReportsWarning(string? version)
    {
        string file = "sarif.log";
        file += version != null ? $",version={version}" : string.Empty;

        ProjectCreator.Templates.SimpleBuild()
            .PropertyGroup()
                .Property("ErrorLog", file)
            .Save(Path.Combine(TestRootPath, "project.csproj"))
            .TryBuild(restore: true, out bool result, out BuildOutput output);

        result.Should().BeTrue();
        // TODO: Implement
        // await Verify(output.WarningEvents);
        // output.WarningEvents.Should().ContainSingle(e =>
        //     e != null && e.Message != null && e.Message.Contains("SARIF log is in v1 format"));
    }

    [Theory]
    [InlineData("2")]
    [InlineData("2.1")]
    public void V2ErrorLogNoWarning(string version)
    {
        ProjectCreator.Templates.SimpleBuild()
            .PropertyGroup()
                .Property("ErrorLog", $"sarif.log,version={version}")
            .Save(Path.Combine(TestRootPath, "project.csproj"))
            .TryBuild(restore: true, out bool result, out BuildOutput output);

        result.Should().BeTrue();
        output.WarningEvents.Should().BeEmpty();
    }
}
