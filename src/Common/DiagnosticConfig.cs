using System.Diagnostics.CodeAnalysis;

namespace SquiggleCop.Common;

/// <summary>
/// A data object that describes the configuration of an analyzer's diagnostic.
/// </summary>
/// <param name="Id">The ID of the diagnostic rule</param>
/// <param name="Title">The short title of the diagnostic rule</param>
/// <param name="Category">The category of the diganostic rule (e.g. Performance, Maintainability, etc.)</param>
/// <param name="DefaultSeverity">The default severity of the diagnostic rule (as specified by the analyzer)</param>
/// <param name="IsEnabledByDefault">If the diagnostic rule is enabled by default (as specified by the analyzer)</param>
/// <param name="EffectiveSeverities">The severities of the diagnostic rule one all build options have been applied (e.g. NoWarn, .editorconfig, rulesets, etc.)</param>
/// <param name="IsEverSuppressed">If the diagnostic rule had either a source suppression or was disabled for part or whole of the compilation via options.</param>
public record struct DiagnosticConfig(string Id, string Title, string Category, DiagnosticSeverity DefaultSeverity, bool IsEnabledByDefault, IReadOnlyCollection<DiagnosticSeverity> EffectiveSeverities, bool IsEverSuppressed);

/// <summary>
/// The severity of a diagnostic.
/// </summary>
[SuppressMessage("Design", "MA0048:File name must match type name", Justification = "Only used with the config and belongs with the config")]
public enum DiagnosticSeverity
{
    /// <summary>
    /// The diagnostic is off / disabled.
    /// </summary>
    None,

    /// <summary>
    /// Informational / note level.
    /// </summary>
    Note,

    /// <summary>
    /// Warning level.
    /// </summary>
    Warning,

    /// <summary>
    /// Error level.
    /// </summary>
    Error,
}
