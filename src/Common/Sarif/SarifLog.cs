using System.Text.Json.Serialization;
using System.Text.Json;

namespace SquiggleCop.Common.Sarif;

internal sealed class SarifLog
{
    public IList<Run>? Runs { get; set; }
    public string? Version { get; set; }
}
