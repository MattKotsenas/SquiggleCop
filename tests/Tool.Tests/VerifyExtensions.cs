using CliWrap;

namespace SquiggleCop.Tool.Tests;

internal static class VerifyExtensions
{
    public static SettingsTask IgnoreCommandResultTimes(this SettingsTask settings)
    {
        settings.CurrentSettings.IgnoreMembers(typeof(CommandResult), "StartTime", "ExitTime", "RunTime");

        return settings;
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
