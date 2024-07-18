using Microsoft.Build.Framework;
using Microsoft.Build.Utilities.ProjectCreation;

namespace SquiggleCop.Common.Tests;

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
}
