using Microsoft.Build.Framework;
using Microsoft.Build.Utilities.ProjectCreation;

namespace SquiggleCop.Tasks.Tests;

public class ErrorLogPropertyDefaultValueTests : TestBase
{
    [Fact]
    public void NotExplicitlySet()
    {
        ProjectCreator project = ProjectCreator.Templates.SimpleBuild()
            .Target(name: "OutputErrorLog", beforeTargets: "AfterCompile")
                .TaskMessage("ErrorLog=$(ErrorLog)", importance: MessageImportance.High);

        project
            .Save(Path.Combine(TestRootPath, "project.csproj"))
            .TryBuild(restore: true, out bool result, out BuildOutput output);

        string[] logs = output.Messages.Where(message => message.Contains("ErrorLog=")).ToArray();
        logs.Should().ContainSingle().Which.Should().Be($"ErrorLog=obj{Path.DirectorySeparatorChar}Debug{Path.DirectorySeparatorChar}net8.0{Path.DirectorySeparatorChar}project.sarif,version=2.1");
    }

    [Fact]
    public void ExplicitlySet()
    {
        ProjectCreator project = ProjectCreator.Templates.SimpleBuild()
            .PropertyGroup()
                .ErrorLog("explicit.log", "2.1")
            .Target(name: "OutputErrorLog", beforeTargets: "AfterCompile")
                .TaskMessage("ErrorLog=$(ErrorLog)", importance: MessageImportance.High);

        project
            .Save(Path.Combine(TestRootPath, "project.csproj"))
            .TryBuild(restore: true, out bool result, out BuildOutput output);

        string[] logs = output.Messages.Where(message => message.Contains("ErrorLog=")).ToArray();
        logs.Should().ContainSingle().Which.Should().Be($"ErrorLog=explicit.log,version=2.1");
    }
}
