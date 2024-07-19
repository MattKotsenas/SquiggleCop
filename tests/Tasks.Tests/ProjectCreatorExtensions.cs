using System.Diagnostics.CodeAnalysis;

using Microsoft.Build.Utilities.ProjectCreation;

namespace SquiggleCop.Tasks.Tests;

internal static class ProjectCreatorExtensions
{
    public static ProjectCreator TouchFilesTask(this ProjectCreator projectCreator, IReadOnlyCollection<string> paths, bool createIfNeeded = true)
    {
        return projectCreator
            .Task(
                name: "Touch",
                parameters: new Dictionary<string, string?>(StringComparer.Ordinal) { { "Files", string.Join(';', paths) }, { "AlwaysCreate", createIfNeeded.ToString().ToLowerInvariant() } });
    }

    public static ProjectCreator CopyFileTask(this ProjectCreator projectCreator, string sourcePath, string destinationPath)
    {
        return projectCreator
            .Task(
                name: "Copy",
                parameters: new Dictionary<string, string?>(StringComparer.Ordinal) { { "SourceFiles", sourcePath }, { "DestinationFiles", destinationPath } });
    }

    public static ProjectCreator ErrorLog(this ProjectCreator projectCreator, string file, string? version = null)
    {
        string value = file;

        if (version is not null)
        {
            value += $",version={version}";
        }

        return projectCreator.Property("ErrorLog", value);
    }

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