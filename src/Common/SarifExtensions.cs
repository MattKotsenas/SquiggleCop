using Microsoft.CodeAnalysis.Sarif;

namespace SquiggleCop.Common;

internal static class SarifExtensions
{
    // Fist version that has the fix for https://github.com/dotnet/roslyn/issues/73070.
    private static readonly Version Roslyn73070FixedVersion = new(4, 10, 0);

    public static T OrDefault<T>(this T? value) where T : ISarifNode, new()
    {
        return value ?? new T();
    }

    public static T GetPropertyOrDefault<T>(this IPropertyBagHolder bag, string name, T defaultValue)
    {
        ArgumentNullException.ThrowIfNull(bag);
        ArgumentNullException.ThrowIfNull(name);

        return bag.TryGetProperty(name, out T value) ? value : defaultValue;
    }

    public static string GetTitle(this ReportingDescriptor rule, Version compilerVersion)
    {
        ArgumentNullException.ThrowIfNull(rule);

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
        ArgumentNullException.ThrowIfNull(run);

        return [.. run.Tool?.Driver?.Rules ?? []];
    }

    public static IReadOnlyDictionary<string, IReadOnlyCollection<ConfigurationOverride>> GetConfigurationOverrides(this Run run)
    {
        ArgumentNullException.ThrowIfNull(run);

        IList<Invocation> invocations = run.Invocations ?? [];

        return invocations
            .Where(i => i.RuleConfigurationOverrides is not null)
            .SelectMany(i => i.RuleConfigurationOverrides)
            .GroupBy(configOverride => configOverride.Descriptor.Id, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.ToList() as IReadOnlyCollection<ConfigurationOverride>, StringComparer.OrdinalIgnoreCase);
    }

    public static FailureLevel GetEffectiveSeverity(this ReportingConfiguration rc)
    {
        ArgumentNullException.ThrowIfNull(rc);

        return !rc.Enabled ? FailureLevel.None : rc.Level;
    }
}
