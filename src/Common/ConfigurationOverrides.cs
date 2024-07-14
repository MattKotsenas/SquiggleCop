using System.Collections.Immutable;

using Microsoft.CodeAnalysis.Sarif;

namespace SquiggleCop.Common;

 // TODO: Consider implementing KVP<string, IEnumerable<ConfigurationOverride>> or similar

internal record class ConfigurationOverrides
{
    public string RuleId { get; init; }
    public ImmutableArray<ConfigurationOverride> Overrides { get; init; } = [];
}