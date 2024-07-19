using System.Diagnostics.CodeAnalysis;

using Microsoft.Build.Utilities.ProjectCreation;

namespace SquiggleCop.Tasks.Tests;

internal static class ProjectCreatorExtensions
{
    public static ProjectCreator UsingRoslynCodeTask(
        this ProjectCreator projectCreator,
        string taskName,
        [StringSyntax("c#")] string code,
        Action<ProjectCreator>? configureUsingTask = null)
    {
        projectCreator.UsingTaskAssemblyFile(
            taskName,
            assemblyFile: "$(MSBuildToolsPath)/Microsoft.Build.Tasks.Core.dll",
            taskFactory: "RoslynCodeTaskFactory");

        if (configureUsingTask is not null)
        {
            configureUsingTask(projectCreator);
        }

        projectCreator.UsingTaskBody(
            $"""
            <Using Namespace="System"/>
            <Using Namespace="System.IO"/>
            <Code Type="Fragment" Language="cs">
            <![CDATA[
            {code}
            ]]>
            </Code>
            """);

        return projectCreator;
    }
}