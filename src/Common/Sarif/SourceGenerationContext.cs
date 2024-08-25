using System.Text.Json.Serialization;
using System.Text.Json;

namespace SquiggleCop.Common.Sarif;

[JsonSourceGenerationOptions(
    AllowTrailingCommas = true,
    ReadCommentHandling = JsonCommentHandling.Skip,
    PropertyNameCaseInsensitive = true,
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
    Converters =
    [
        typeof(JsonStringEnumConverter<FailureLevel>),
        typeof(BooleanConverter),
    ])]
[JsonSerializable(typeof(SarifLog))]
internal sealed partial class SourceGenerationContext : JsonSerializerContext;
