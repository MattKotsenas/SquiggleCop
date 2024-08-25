namespace SquiggleCop.Common.Sarif;

internal sealed class ReportingConfiguration
{
    internal static ReportingConfiguration Default { get; } = new ReportingConfiguration();

    public bool Enabled { get; set; } = true;
    public FailureLevel Level { get; set; } = FailureLevel.Warning;
}
