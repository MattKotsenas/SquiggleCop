using Microsoft.CodeAnalysis.Sarif;

namespace SquiggleCop.Common;

internal static class SarifExtensions
{
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

    public static string GetTitle(this ReportingDescriptor rule)
    {
        ArgumentNullException.ThrowIfNull(rule);

        // TODO: Only run this if the compiler version is affected

        // Normalize the title of rules that have multiple possible names.
        // Remove this when https://github.com/dotnet/roslyn/issues/73070 is fixed.
        return rule.Id switch
        {
            "IDE0053" => "Use expression body for lambdas",
            "IDE0073" => "Require file header",
            _ => rule.ShortDescription.TextOrDefault(string.Empty),
        };
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

    public static IReadOnlyDictionary<string, List<ConfigurationOverride>> GetConfigurationOverrides(this Run run)
    {
        ArgumentNullException.ThrowIfNull(run);

        // TODO: Use ConfigurationOverrides type instead

        StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;

        if (run.Invocations is null)
        {
            return new Dictionary<string, List<ConfigurationOverride>>(stringComparer);
        }

        return run.Invocations
            .Where(i => i.RuleConfigurationOverrides is not null)
            .SelectMany(i => i.RuleConfigurationOverrides)
            .GroupBy(configOverride => configOverride.Descriptor.Id, stringComparer)
            .ToDictionary(group => group.Key, group => group.ToList(), stringComparer);
    }

    public static FailureLevel GetEffectiveSeverity(this ReportingConfiguration rc)
    {
        ArgumentNullException.ThrowIfNull(rc);

        return !rc.Enabled ? FailureLevel.None : rc.Level;
    }

    public static bool IsCSharpCompiler(this ToolComponent? tool)
    {
        return string.Equals(tool?.Name, "Microsoft (R) Visual C# Compiler", StringComparison.Ordinal);
    }
}
