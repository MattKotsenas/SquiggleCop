namespace SquiggleCop.Tool;

internal class SarifInvalidException : ExitCodeException
{
    protected SarifInvalidException()
    {
    }

    public SarifInvalidException(string message) : base(message) { }

    public SarifInvalidException(string message, Exception? innerException) : base(message, innerException) { }

    public override int ExitCode => -11;
}
