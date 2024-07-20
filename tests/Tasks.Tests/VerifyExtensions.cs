using Microsoft.Build.Framework;
using Microsoft.Build.Utilities.ProjectCreation;

namespace SquiggleCop.Tasks.Tests;

internal static class VerifyExtensions
{
    public static IEnumerable<BuildLogMessage> ToBuildLogMessages(this IEnumerable<BuildWarningEventArgs> args)
    {
        foreach (var arg in args)
        {
            yield return new("Warning", arg.Code, arg.Message);
        }
    }

    public static IEnumerable<BuildLogMessage> ToBuildLogMessages(this IEnumerable<BuildErrorEventArgs> args)
    {
        foreach (var arg in args)
        {
            yield return new("Error", arg.Code, arg.Message);
        }
    }

    public static IEnumerable<BuildLogMessage> ToBuildLogMessages(this BuildOutput buildOutput)
    {
        return buildOutput.ErrorEvents.ToBuildLogMessages().Concat(buildOutput.WarningEvents.ToBuildLogMessages());
    }

    public static SettingsTask ScrubDirectory(this SettingsTask settings, string directory, string replacement)
    {
        string backslash = directory.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        string forwardslash = directory.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        settings.CurrentSettings.ScrubLinesWithReplace(l => l.Replace(backslash, replacement, StringComparison.Ordinal));
        settings.CurrentSettings.ScrubLinesWithReplace(l => l.Replace(forwardslash, replacement, StringComparison.Ordinal));

        return settings;
    }

    public static SettingsTask ScrubPathSeparators(this SettingsTask settings)
    {
        settings.CurrentSettings.ScrubLinesWithReplace(l => l.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));

        return settings;
    }
}
