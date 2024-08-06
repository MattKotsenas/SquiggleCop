using Microsoft.Build.Utilities.ProjectCreation;

namespace SquiggleCop.Tasks.Tests;

internal static class ProjectCreatorTemplatesExtensions
{
    public static ProjectCreator SimpleBuild(this ProjectCreatorTemplates templates)
    {
        return templates.SdkCsproj(targetFramework: "net8.0") // Avoid NETStandard to avoid needing to restore reference assemblies (and thus likely a dependency on the NuGet.org feed)
            .ItemGroup()
                .ItemPackageReference("SquiggleCop.Tasks", "*-*"); // Use the latest version (there should only ever be 1), even if it's prerelease
    }
}
