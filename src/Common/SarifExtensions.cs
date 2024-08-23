using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

using SquiggleCop.Common.Sarif;

namespace SquiggleCop.Common;

internal static class SarifExtensions
{
    // Fist version that has the fix for https://github.com/dotnet/roslyn/issues/73070.
    private static readonly Version Roslyn73070FixedVersion = new(4, 11, 0);

    public static string GetTitle(this ReportingDescriptor rule, Version compilerVersion)
    {
        Guard.ThrowIfNull(rule);

        if (compilerVersion < Roslyn73070FixedVersion)
        {
            // Normalize the title of rules that have multiple possible names.
            // Remove this when https://github.com/dotnet/roslyn/issues/73070 is fixed.
            if (string.Equals(rule.Id, "IDE0053", StringComparison.Ordinal))
            {
                return "Use expression body for lambdas";
            }

            if (string.Equals("IDE0073", rule.Id, StringComparison.Ordinal))
            {
                return "Require file header";
            }
        }

        return rule.ShortDescription.TextOrDefault(string.Empty);
    }

    public static string TextOrDefault(this MultiformatMessageString? message, string defaultValue)
    {
        return message?.Text ?? defaultValue;
    }

    public static IReadOnlyCollection<ReportingDescriptor> GetRules(this Run run)
    {
        Guard.ThrowIfNull(run);

        IList<ReportingDescriptor> rules = run.Tool?.Driver?.Rules ?? [];
        return new ReadOnlyCollection<ReportingDescriptor>(rules);
    }

    public static IReadOnlyDictionary<string, IReadOnlyCollection<ConfigurationOverride>> GetConfigurationOverrides(this Run run)
    {
        Guard.ThrowIfNull(run);

        IList<Invocation> invocations = run.Invocations ?? Array.Empty<Invocation>();

        return invocations
            .SelectMany(i => i.RuleConfigurationOverrides ?? Array.Empty<ConfigurationOverride>())
            .Where(configOverride => configOverride.Descriptor?.Id is not null)
            .GroupBy(configOverride => configOverride.Descriptor!.Id!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.ToList() as IReadOnlyCollection<ConfigurationOverride>, StringComparer.OrdinalIgnoreCase);
    }

    public static FailureLevel GetEffectiveSeverity(this ReportingConfiguration rc)
    {
        Guard.ThrowIfNull(rc);

        return !rc.Enabled ? FailureLevel.None : rc.Level;
    }

    public static DiagnosticSeverity ToDiagnosticSeverity(this FailureLevel failureLevel)
    {
        return failureLevel switch
        {
            FailureLevel.Error => DiagnosticSeverity.Error,
            FailureLevel.Warning => DiagnosticSeverity.Warning,
            FailureLevel.Note => DiagnosticSeverity.Note,
            FailureLevel.None => DiagnosticSeverity.None,
            _ => throw new ArgumentOutOfRangeException(nameof(failureLevel)),
        };
    }

    public static bool IsCSharpCompiler(this ToolComponent? tool)
    {
        return string.Equals(tool?.Name, "Microsoft (R) Visual C# Compiler", StringComparison.Ordinal);
    }

    public static bool TryGetVersion(this ToolComponent? tool, [NotNullWhen(true)] out Version? version)
    {
        return Version.TryParse(tool?.SemanticVersion, out version);
    }
}
