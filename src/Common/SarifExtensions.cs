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

        if (bag.TryGetProperty(name, out T value))
        {
            return value;
        }

        return defaultValue;
    }

    public static string GetTitle(this ReportingDescriptor rule)
    {
        ArgumentNullException.ThrowIfNull(rule);

        // TODO: Only run this if the compiler version is affected

        // Normalize the title of rules that have multiple possible names.
        // Remove this when https://github.com/dotnet/roslyn/issues/73070 is fixed.
        switch (rule.Id)
        {
            case "IDE0053":
                return "Use expression body for lambdas";
            case "IDE0073":
                return "Require file header";
            default:
                return rule.ShortDescription.TextOrDefault(string.Empty);
        }
    }

    public static string TextOrDefault(this MultiformatMessageString? message, string defaultValue)
    {
        if (message?.Text is not null)
        {
            return message.Text;
        }

        return defaultValue;
    }

    public static IReadOnlyCollection<ReportingDescriptor> GetRules(this Run run)
    {
        ArgumentNullException.ThrowIfNull(run);

        if (run.Tool?.Driver?.Rules is null)
        {
            return Array.Empty<ReportingDescriptor>();
        }

        return run.Tool.Driver.Rules.ToList();
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

        if (!rc.Enabled)
        {
            return FailureLevel.None;
        }

        return rc.Level;
    }

    public static bool IsCSharpCompiler(this ToolComponent? tool)
    {
        return string.Equals(tool?.Name, "Microsoft (R) Visual C# Compiler", StringComparison.Ordinal);
    }
}
