using Microsoft.Build.Utilities.ProjectCreation;

namespace SquiggleCop.Tasks.Tests;

internal static class ProjectCreatorTemplatesExtensions
{
    public static ProjectCreator SimpleBuild(this ProjectCreatorTemplates templates)
    {
        return templates.SdkCsproj()
            .ItemGroup()
                .ItemPackageReference("SquiggleCop.Tasks", "*-*"); // Use the latest version (there should only ever be 1), even if it's prerelease
    }
}
