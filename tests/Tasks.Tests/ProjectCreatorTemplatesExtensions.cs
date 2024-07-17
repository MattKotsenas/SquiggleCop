using Microsoft.Build.Utilities.ProjectCreation;

namespace SquiggleCop.Common.Tests;

internal static class ProjectCreatorTemplatesExtensions
{
    public static ProjectCreator SimpleBuild(this ProjectCreatorTemplates templates)
    {
        // A simple project that only provides the basic hooks of a dotnet build

        return ProjectCreator.Create(defaultTargets: "Build")
            .Target(name: "Build", dependsOnTargets: "BeforeBuild;CoreBuild;AfterBuild")
            .Target(name: "BeforeBuild")
            .Target(name: "CoreBuild")
            .Target(name: "AfterBuild");
    }
}
