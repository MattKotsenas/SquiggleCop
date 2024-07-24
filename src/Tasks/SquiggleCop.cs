using System;

using Microsoft.Build.Framework;

using Newtonsoft.Json;

using SquiggleCop.Common;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

using Task = Microsoft.Build.Utilities.Task;

namespace SquiggleCop.Tasks;

/// <summary>
/// A task that creates a baseline of Roslyn diagnostics from a given SARIF log file.
/// </summary>
public class SquiggleCop : Task
{
    private readonly SarifParser _parser = new();
    private readonly ISerializer _serializer = new SerializerBuilder()
        .WithNamingConvention(PascalCaseNamingConvention.Instance)
        .Build();

    /// <summary>
    /// The SARIF log file to create a baseline from. Must be in SARIF v2.1 format.
    /// </summary>
    public string? ErrorLog { get; set; }

    /// <summary>
    /// If <see langword="true"/>, the baseline file will be automatically created / updated.
    /// If <see langword="false"/>, the task will warn if the existing baseline file does not match the new baseline.
    /// </summary>
    [Required]
    public bool AutoBaseline { get; set; } = false;

    /// <summary>
    /// The baseline file to write to. If this collection contains more than one item a warning is logged
    /// and the first item is used.
    /// </summary>
    public ITaskItem[] BaselineFiles { get; set; } = [];

    /// <summary>
    /// Create a baseline of Roslyn diagnostics from a given SARIF log file.
    /// </summary>
    /// <returns><see langword="true" />, if successful</returns>
    public override bool Execute()
    {
        if (ErrorLog is null)
        {
            LogWarning(warningCode: DiagnosticIds.Sarif.NotSpecified, "ErrorLog property not specified");
            return true;
        }

        if (!File.Exists(ErrorLog))
        {
            LogWarning(warningCode: DiagnosticIds.Sarif.NotFound, "SARIF log file not found: {0}", ErrorLog);
            return true;
        }

        string baselineFile = ValidateBaselineFile(BaselineFiles);

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

            string newBaseline = _serializer.Serialize(configs);

            if (AreDifferent(baselineFile, newBaseline))
            {
                if (AutoBaseline)
                {
                    WriteBaselineFile(baselineFile, newBaseline);
                }
                else
                {
                    LogWarning(warningCode: DiagnosticIds.Baseline.Mismatch, "Baseline mismatch: {0}", baselineFile);
                }
            }

            return true;
        }
        catch (UnsupportedVersionException)
        {
            LogWarning(warningCode: DiagnosticIds.Sarif.V1Format, "SARIF log '{0}' is in v1 format; SquiggleCop requires SARIF v2.1 logs", ErrorLog);
            return true;
        }
    }

    private static void WriteBaselineFile(string path, string contents)
    {
        string? parent = Directory.GetParent(path)?.FullName;
        if (parent is not null && !Directory.Exists(parent))
        {
            Directory.CreateDirectory(parent);
        }

        File.WriteAllText(path, contents);
    }

    private string ValidateBaselineFile(ITaskItem[] baselineFiles)
    {
        if (baselineFiles.Length > 1)
        {
            LogWarning(DiagnosticIds.Baseline.AmbiguousReference, "Multiple baseline files found in @(AdditionalFiles); using the first one found: {0}", baselineFiles[0]);
        }

        return baselineFiles[0].ItemSpec;
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
}
