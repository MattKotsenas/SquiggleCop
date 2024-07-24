namespace SquiggleCop.Tool;

internal class SarifNotFoundException : ExitCodeException
{
    private const string MessageFormat = " SARIF log file not found: '{0}'";

    public override int ExitCode { get; } = -10;

    protected SarifNotFoundException()
    {
    }

    public SarifNotFoundException(string file) : base(string.Format(MessageFormat, file))
    {
    }

    public SarifNotFoundException(string file, Exception? innerException) : base(string.Format(MessageFormat, file), innerException)
    {
    }
}
