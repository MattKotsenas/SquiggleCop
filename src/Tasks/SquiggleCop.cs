using SquiggleCop.Common;

using Task = Microsoft.Build.Utilities.Task;

namespace SquiggleCop.Tasks;

/// <summary>
/// A task that creates a baseline of Roslyn diagnostics from a given SARIF log file.
/// </summary>
public class SquiggleCop : Task
{
    // Error code pattern:
    //   - SQ1XXX: SARIF file parsing
    private static readonly string LogNotSpecified = "SQ1000";
    private static readonly string LogNotFound = "SQ1001";
    private static readonly string LogV1Format = "SQ1002";

    private readonly SarifParser _parser = new();

    /// <summary>
    /// The SARIF log file to create a baseline from. Must be in SARIF v2.1 format.
    /// </summary>
    public string? ErrorLog { get; set; }

    /// <summary>
    /// Create a baseline of Roslyn diagnostics from a given SARIF log file.
    /// </summary>
    /// <returns><see langword="true" />, if successful</returns>
    public override bool Execute()
    {
        ErrorLog = TrimSarifVersion(ErrorLog);

        if (ErrorLog is null)
        {
            LogWarning(warningCode: LogNotSpecified, "ErrorLog property not specified");
            return true;
        }

        if (!File.Exists(ErrorLog))
        {
            LogWarning(warningCode: LogNotFound, "SARIF log file not found: {0}", ErrorLog);
            return true;
        }

        try
        {
            using Stream stream = File.OpenRead(ErrorLog!);
            _ = _parser.Parse(stream);
            return true;
        }
        catch (UnsupportedVersionException)
        {
            LogWarning(warningCode: LogV1Format, "SARIF log '{0}' is in v1 format; SquiggleCop requires SARIF v2.1 logs", ErrorLog);
            return true;
        }
    }

    private void LogWarning(string warningCode, string message, params object[] messageArgs)
    {
        string helpLink = $"https://github.com/MattKotsenas/SquiggleCop/tree/{ThisAssembly.GitCommitId}/docs/{warningCode}.md";
        Log.LogWarning(subcategory: null, warningCode, helpKeyword: null, helpLink, file: null, lineNumber: 0, columnNumber: 0, endLineNumber: 0, endColumnNumber: 0, message, messageArgs);
    }

    private static string? TrimSarifVersion(string? errorLog)
    {
        if (errorLog is null) { return errorLog; }

        int index = errorLog.LastIndexOf(",version=", StringComparison.OrdinalIgnoreCase);
        if (index > -1)
        {
            errorLog = errorLog[..index];
        }

        return errorLog;
    }
}
