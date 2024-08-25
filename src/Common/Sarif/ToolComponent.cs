namespace SquiggleCop.Common.Sarif;

internal sealed class ToolComponent
{
    public string? Name { get; set; }
    public string? SemanticVersion { get; set; }
    public IList<ReportingDescriptor>? Rules { get; set; }
}
