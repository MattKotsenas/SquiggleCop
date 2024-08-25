namespace SquiggleCop.Common.Sarif;

internal sealed class ReportingDescriptor
{
    public string? Id { get; set; }
    public ReportingConfiguration DefaultConfiguration { get; set; } = ReportingConfiguration.Default;
    public MultiformatMessageString? ShortDescription { get; set; }
    public Properties? Properties { get; set; }
}
