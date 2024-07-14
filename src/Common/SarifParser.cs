using Microsoft.CodeAnalysis.Sarif;
using Newtonsoft.Json;

namespace SquiggleCop.Common;

public class SarifParser
{
    // TODO: Note that the stream is borrowed here and won't be closed

    public async Task<IReadOnlyCollection<DiagnosticConfig>> ParseAsync(Stream stream)
    {
        try
        {
            // TODO: Should we use deferred loading?
            SarifLog log = SarifLog.Load(stream, deferred: false);

            // TODO: Assert minimum version of compiler

            return log.Runs.SelectMany(ParseRun).ToList();
        }
        catch (JsonSerializationException e)
        {
            // TODO: Find stream-compatible way to test for SARIF v1
            // if (await IsVersion1FileAsync(path).ConfigureAwait(false))
            // {
            //     throw new InvalidDataException($"File '{path}' appears to be a SARIF v1 file. See https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-options/errors-warnings#errorlog to enable SARIF v2 logs.", e);
            // }

            throw new InvalidDataException($"Unable to parse SARIF log. See inner exception for details", e);
        }
    }

    private IEnumerable<DiagnosticConfig> ParseRun(Run run)
    {
        IReadOnlyCollection<ReportingDescriptor> rules = run.GetRules();
        IReadOnlyDictionary<string, List<ConfigurationOverride>> configurationOverrides = run.GetConfigurationOverrides();

        foreach (ReportingDescriptor rule in rules)
        {
            ReportingConfiguration defaultConfiguration = rule.DefaultConfiguration.OrDefault();

            string defaultSeverity = defaultConfiguration.Level.ToString();
            string[] effectiveSeverities = [defaultConfiguration.GetEffectiveSeverity().ToString()];

            if (configurationOverrides.TryGetValue(rule.Id, out List<ConfigurationOverride>? co))
            {
                ReportingConfiguration[] rcs = co.Select(c => c.Configuration.OrDefault()).ToArray();
                effectiveSeverities = rcs.Select(rc => rc.GetEffectiveSeverity().ToString()).ToArray();
            }

            yield return new DiagnosticConfig(
                id: rule.Id,
                title: rule.GetTitle(),
                category: rule.GetPropertyOrDefault("category", string.Empty),
                defaultSeverity: defaultSeverity,
                isEnabledByDefault: defaultConfiguration.Enabled,
                effectiveSeverities: effectiveSeverities);
        }
    }

    private async Task<bool> IsVersion1FileAsync(string path)
    {
        using var sr = new StreamReader(path);

        string? line;
        while ((line = await sr.ReadLineAsync().ConfigureAwait(false)) is not null)
        {
            if (line.Contains("http://json.schemastore.org/sarif-1.0.0", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
