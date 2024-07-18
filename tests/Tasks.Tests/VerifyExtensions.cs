using Microsoft.Build.Framework;

namespace SquiggleCop.Common.Tests;

internal static class VerifyExtensions
{
    public static IEnumerable<BuildLogMessage> ToBuildLogMessages(this IEnumerable<BuildWarningEventArgs> args)
    {
        foreach (var arg in args)
        {
            yield return new(arg.Code, arg.Message);
        }
    }
}
