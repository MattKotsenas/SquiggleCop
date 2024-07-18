using Microsoft.Build.Utilities.ProjectCreation;

namespace SquiggleCop.Common.Tests;

internal static class ProjectCreatorTemplatesExtensions
{
    public static ProjectCreator SimpleBuild(this ProjectCreatorTemplates templates)
    {
        return templates.SdkCsproj()
            .ItemPackageReference("SquiggleCop.Tasks", "*");
    }
}
