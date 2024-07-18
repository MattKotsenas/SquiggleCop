using System.Runtime.Serialization;

namespace SquiggleCop.Common;

/// <summary>
/// Represents an error when trying to parse a SARIF log with an unsupported version.
/// </summary>
public class UnsupportedVersionException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnsupportedVersionException"/> class.
    /// </summary>
    public UnsupportedVersionException() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnsupportedVersionException"/> class with a specified error message.
    /// </summary>
    /// <param name="message"></param>
    public UnsupportedVersionException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the System.Exception class with a specified error
    /// message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="inner">
    /// The exception that is the cause of the current exception, or a <see langword="null" /> reference
    /// if no inner exception is specified.
    /// </param>
    public UnsupportedVersionException(string message, Exception inner) : base(message, inner) { }
}
