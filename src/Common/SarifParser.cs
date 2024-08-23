using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;

using Microsoft.CodeAnalysis.Sarif;

using Newtonsoft.Json;

namespace SquiggleCop.Common;

/// <summary>
/// Parses SARIF logs to extract the diagnostic configurations.
/// </summary>
public class SarifParser
{
    private static readonly Version MinimumCompilerVersion = new(4, 8, 0);
    private const int MaxDiagnosticSeverities = 4; // Keep in-sync with the value of `Enum.GetValues(typeof(DiagnosticSeverity)).Length`

    /// <summary>
    /// Parses the SARIF log from the given stream and returns the diagnostic configurations.
    /// </summary>
    /// <param name="stream">
    /// A <paramref name="stream"/> that contains the SARIF log.
    /// </param>
    /// <returns>
    /// A collection of <see cref="DiagnosticConfig"/> objects that represent the diagnostic configurations.
    /// </returns>
    /// <remarks>
    /// The <paramref name="stream"/> is borrowed and will not be closed / disposed.
    /// </remarks>
    /// <exception cref="InvalidDataException">
    /// Throws an <see cref="UnsupportedVersionException"/> if the SARIF log cannot be parsed.
    /// </exception>
    public IReadOnlyCollection<DiagnosticConfig> Parse(Stream stream)
    {
        Guard.ThrowIfNull(stream);
        if (!stream.CanRead) { throw new ArgumentException("Stream must be readable", nameof(stream)); }
        if (!stream.CanSeek) { throw new ArgumentException("Stream must be seekable", nameof(stream)); }

        SarifLog log;
        try
        {
            log = SarifLog.Load(stream, deferred: true);
        }
        catch (JsonSerializationException e) when (e.Message.Contains("Required property 'driver' not found in JSON"))
        {
            throw new UnsupportedVersionException("Contents appear to be a SARIF v1 file. See https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-options/errors-warnings#errorlog to enable SARIF v2 logs.", e);
        }

        return log
            .Runs
            .SelectMany(ParseRun)
            .OrderBy(dc => dc.Id, StringComparer.InvariantCulture)
            .ThenBy(dc => dc.Title, StringComparer.InvariantCulture)
            .ToList();
    }

    /// <inheritdoc />
    [SuppressMessage("Design", "MA0042:Do not use blocking calls in an async method", Justification = "This is provided for symmetry, as I'm assuming eventually the API will become actually async.")]
    public Task<IReadOnlyCollection<DiagnosticConfig>> ParseAsync(Stream stream)
    {
        return Task.FromResult(Parse(stream));
    }

    private IEnumerable<DiagnosticConfig> ParseRun(Run run)
    {
        ToolComponent? compiler = run.Tool?.Driver;

        if (!compiler.IsCSharpCompiler())
        {
            yield break;
        }

        if (!compiler.TryGetVersion(out Version? version))
        {
            throw new UnsupportedVersionException("Unable to parse compiler version. Ensure you are using SDK 8.0.100 or later.");
        }

        if (version < MinimumCompilerVersion)
        {
            throw new UnsupportedVersionException($"Compiler version '{version}' is less than minimum required version '{MinimumCompilerVersion}'. Ensure you are using SDK 8.0.100 or later.");
        }

        IReadOnlyCollection<ReportingDescriptor> rules = run.GetRules();
        IReadOnlyDictionary<string, IReadOnlyCollection<ConfigurationOverride>> configurationOverrides = run.GetConfigurationOverrides();

        foreach (ReportingDescriptor rule in rules)
        {
            ReportingConfiguration defaultConfiguration = rule.DefaultConfiguration.OrDefault();

            DiagnosticSeverity defaultSeverity = defaultConfiguration.Level.ToDiagnosticSeverity();

            HashSet<DiagnosticSeverity> effectiveSeverities = new(MaxDiagnosticSeverities)
            {
                defaultConfiguration.GetEffectiveSeverity().ToDiagnosticSeverity(),
            };

            if (configurationOverrides.TryGetValue(rule.Id, out IReadOnlyCollection<ConfigurationOverride>? cos))
            {
                effectiveSeverities.Clear();

                foreach (ConfigurationOverride co in cos)
                {
                    effectiveSeverities.Add(co.Configuration.OrDefault().GetEffectiveSeverity().ToDiagnosticSeverity());
                }
            }

            yield return new DiagnosticConfig()
            {
                Id = rule.Id,
                Title = rule.GetTitle(version),
                Category = rule.GetPropertyOrDefault("category", defaultValue: string.Empty),
                DefaultSeverity = defaultSeverity,
                IsEnabledByDefault = defaultConfiguration.Enabled,
                EffectiveSeverities = effectiveSeverities.ToArray(),
                IsEverSuppressed = rule.GetPropertyOrDefault("isEverSuppressed", defaultValue: false),
            };
        }
    }
}
