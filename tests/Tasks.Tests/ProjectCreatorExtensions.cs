using System.Diagnostics.CodeAnalysis;
using System.Text;

using Microsoft.Build.Utilities.ProjectCreation;

namespace SquiggleCop.Tasks.Tests;

internal static class ProjectCreatorExtensions
{
    public static ProjectCreator AdditionalFiles(this ProjectCreator projectCreator, string path)
    {
       return projectCreator.ItemInclude("AdditionalFiles", path);
    }

    public static ProjectCreator MakeDirTask(this ProjectCreator projectCreator, IReadOnlyCollection<string> paths)
    {
        return projectCreator
            .Task(
                name: "MakeDir",
                parameters: new Dictionary<string, string?>(StringComparer.Ordinal) { { "Directories", string.Join(';', paths) } });
    }

    public static ProjectCreator TouchFilesTask(this ProjectCreator projectCreator, IReadOnlyCollection<string> paths, bool createIfNeeded = true, DateTime? lastWriteTime = null)
    {
        Dictionary<string, string?> parameters = new(StringComparer.Ordinal)
        {
            { "Files", string.Join(';', paths) },
            { "ForceTouch", "true" },
            { "AlwaysCreate", createIfNeeded.ToString().ToLowerInvariant() },
        };

        if (lastWriteTime is not null)
        {
            parameters.Add("Time", lastWriteTime.Value.ToString("O"));
        }

        return projectCreator
            .Task(name: "Touch", parameters: parameters);
    }

    public static ProjectCreator CopyFileTask(this ProjectCreator projectCreator, string sourcePath, string destinationPath)
    {
        return projectCreator
            .Task(
                name: "Copy",
                parameters: new Dictionary<string, string?>(StringComparer.Ordinal) { { "SourceFiles", sourcePath }, { "DestinationFiles", destinationPath } });
    }

    public static ProjectCreator WriteLinesToFileTask(this ProjectCreator projectCreator, string path, string lines, Encoding? encoding = null, bool overwrite = true)
    {
        encoding ??= Encoding.UTF8;

        return projectCreator
            .Task(name: "WriteLinesToFile",
            parameters: new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                { "File", path },
                { "Lines", lines },
                { "Overwrite", overwrite.ToString().ToLowerInvariant() },
                { "Encoding", encoding.WebName },
            });
    }

    public static ProjectCreator AutoBaseline(this ProjectCreator projectCreator, bool? autoBaseline = null)
    {
        if (autoBaseline is not null)
        {
            projectCreator
                .Property("SquiggleCop_AutoBaseline", autoBaseline.ToString()!.ToLowerInvariant());
        }

        return projectCreator;
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