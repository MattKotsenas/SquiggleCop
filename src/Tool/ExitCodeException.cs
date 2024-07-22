namespace SquiggleCop.Tool;

/// <summary>
/// Base class for an exception that has an associated exit code.
/// </summary>
public abstract class ExitCodeException : Exception
{
    /// <summary>
    /// The exit code that should be returned if this exception is unhandled.
    /// </summary>
    public abstract int ExitCode { get; }

    /// <inheritdoc/>
    protected ExitCodeException() : base()
    {
    }

    /// <inheritdoc />
    protected ExitCodeException(string message) : base(message)
    {
    }

    /// <inheritdoc />
    protected ExitCodeException(string message, Exception? innerException) : base(message, innerException)
    {
    }
}
