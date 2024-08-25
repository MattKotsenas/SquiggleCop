namespace SquiggleCop.Common.Sarif;

internal sealed class Run
{
    public Tool? Tool { get; set; }
    public IList<Invocation>? Invocations { get; set; }
}
