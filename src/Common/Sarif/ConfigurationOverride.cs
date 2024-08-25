namespace SquiggleCop.Common.Sarif;

internal sealed class ConfigurationOverride
{
    public ReportingDescriptorReference? Descriptor { get; set; }
    public ReportingConfiguration Configuration { get; set; } = ReportingConfiguration.Default;
}
