using Microsoft.Build.Framework;

using Newtonsoft.Json;

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
    //   - SQ2XXX: Baseline file handling
    private static readonly string SarifNotSpecified = "SQ1000";
    private static readonly string SarifNotFound = "SQ1001";
    private static readonly string SarifV1Format = "SQ1002";
    private static readonly string BaselineFileMismatch = "SQ2000";

    private readonly SarifParser _parser = new();

    /// <summary>
    /// The SARIF log file to create a baseline from. Must be in SARIF v2.1 format.
    /// </summary>
    public string? ErrorLog { get; set; }

    /// <summary>
    /// If <see langword="true"/>, the <see cref="BaselineFile"/> with be automatically created / updated. If <see langword="false"/>, the task will fail if the baseline file already exists.
    /// </summary>
    [Required]
    public bool AutoBaseline { get; set; } = true;

    /// <summary>
    /// The name of the baseline file.
    /// </summary>
    public static string BaselineFile { get; } = "SquiggleCop.Baseline.json";

    /// <summary>
    /// Create a baseline of Roslyn diagnostics from a given SARIF log file.
    /// </summary>
    /// <returns><see langword="true" />, if successful</returns>
    public override bool Execute()
    {
        ErrorLog = TrimSarifVersion(ErrorLog);

        if (ErrorLog is null)
        {
            LogWarning(warningCode: SarifNotSpecified, "ErrorLog property not specified");
            return true;
        }

        if (!File.Exists(ErrorLog))
        {
            LogWarning(warningCode: SarifNotFound, "SARIF log file not found: {0}", ErrorLog);
            return true;
        }

        try
        {
            using Stream stream = File.OpenRead(ErrorLog!);

            IReadOnlyCollection<DiagnosticConfig> configs = _parser.Parse(stream);

            // TODO: Rewrite for performance; maybe hash streams?

            // TODO: Use line-by-line comparison to avoid newline handling differences.
            // If we use binary comparison be sure to document the proper procedure
            // for .gitattributes (or whatever).
            //
            // Also, consider that resetting line endings may result in source control churn.

            string newBaseline = JsonConvert.SerializeObject(configs); // TODO: Pretty print the serialized JSON

            if (AreDifferent(BaselineFile, newBaseline))
            {
                if (AutoBaseline)
                {
                    File.WriteAllText(BaselineFile, newBaseline);
                }
                else
                {
                    LogWarning(warningCode: BaselineFileMismatch, "Baseline file mismatch: {0}", BaselineFile);
                }
            }

            return true;
        }
        catch (UnsupportedVersionException)
        {
            LogWarning(warningCode: SarifV1Format, "SARIF log '{0}' is in v1 format; SquiggleCop requires SARIF v2.1 logs", ErrorLog);
            return true;
        }
    }

    private static bool AreDifferent(string path, string contents)
    {
        return !File.Exists(path) || !string.Equals(File.ReadAllText(path), contents, StringComparison.Ordinal);
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
