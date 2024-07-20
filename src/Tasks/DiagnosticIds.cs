namespace SquiggleCop.Tasks;

/// <summary>
/// Diagnostic IDs used by SquiggleCop.
/// </summary>
/// <remarks>
/// This intentionally doesn't use <see langword="const"/> references to avoid potential breaking changes when Ids are updated.
/// </remarks>
internal static class DiagnosticIds
{
    /// <summary>
    /// SQ1XXX: SARIF file parsing
    /// </summary>
    internal static class Sarif
    {
        public static string NotSpecified { get; } = "SQ1000";
        public static string NotFound { get; } = "SQ1001";
        public static string V1Format { get; } = "SQ1002";
    }

    /// <summary>
    /// SQ2XXX: Baseline file handling
    /// </summary>
    internal static class Baseline
    {
        public static string Mismatch { get; } = "SQ2000";
        public static string AmbiguousReference { get; } = "SQ2001";
    }
}
