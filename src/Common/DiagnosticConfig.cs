namespace SquiggleCop.Common;

/// <summary>
/// A data object that describes the configuration of an analyzer's diagnostic.
/// </summary>
public record DiagnosticConfig
{
    /// <summary>
    /// Create a new instance of <see cref="DiagnosticConfig"/>.
    /// </summary>
    /// <param name="id">The ID of the diagnostic rule</param>
    /// <param name="title">The short title of the diagnostic rule</param>
    /// <param name="category">The category of the diganostic rule (e.g. Performance, Maintainability, etc.)</param>
    /// <param name="defaultSeverity">The default severity of the diagnostic rule (as specified by the analyzer)</param>
    /// <param name="isEnabledByDefault">If the diagnostic rule is enabled by default (as specified by the analyzer)</param>
    /// <param name="effectiveSeverities">The severities of the diagnostic rule one all build options have been applied (e.g. NoWarn, .editorconfig, rulesets, etc.)</param>
    /// <param name="isEverSuppressed">If the diagnostic rule had either a source suppression or was disabled for part or whole of the compilation via options.</param>
    public DiagnosticConfig(string id, string title, string category, string defaultSeverity, bool isEnabledByDefault, IReadOnlyCollection<string> effectiveSeverities, bool isEverSuppressed)
    {
        Id = id;
        Title = title;
        Category = category;
        DefaultSeverity = defaultSeverity;
        IsEnabledByDefault = isEnabledByDefault;
        EffectiveSeverities = effectiveSeverities;
        IsEverSuppressed = isEverSuppressed;
    }

    /// <summary>
    /// The ID of the diagnostic rule.
    /// </summary>
    public string Id { get; init; }

    /// <summary>
    /// The short title of the diagnostic rule.
    /// </summary>
    public string Title { get; init; }

    /// <summary>
    /// The category of the diagnostic rule (e.g. Performance, Maintainability, etc.).
    /// </summary>
    public string Category { get; init; }

    /// <summary>
    /// The default severity of the diagnostic rule (as specified by the analyzer).
    /// </summary>
    public string DefaultSeverity { get; init; }

    /// <summary>
    /// The severity of the diagnostic rule one all build options have been applied (e.g. NoWarn, .editorconfig, rulesets, etc.).
    /// </summary>
    public bool IsEnabledByDefault { get; init; }

    /// <summary>
    /// The severities of the diagnostic rule one all build options have been applied (e.g. NoWarn, .editorconfig, rulesets, etc.).
    /// </summary>
    public IReadOnlyCollection<string> EffectiveSeverities { get; init; }

    /// <summary>
    /// If the diagnostic rule had either a source suppression or was disabled for part or whole of the compilation via options.
    /// </summary>
    public bool IsEverSuppressed { get; init; }
}
