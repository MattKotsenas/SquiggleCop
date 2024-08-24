using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

using Microsoft.CodeAnalysis.Sarif;

using Newtonsoft.Json;

namespace SquiggleCop.Common;

/// <summary>
/// Converts a property bag (a JSON object whose keys have arbitrary names and whose values
/// may be any JSON values) into a dictionary whose keys match the JSON object's
/// property names, and whose values are of type <see cref="SerializedPropertyInfo"/>
/// </summary>
internal class PropertyBagConverter : System.Text.Json.Serialization.JsonConverter<IDictionary<string, SerializedPropertyInfo>>
{
    private static readonly SerializedPropertyInfoConverter Instance = new();

    //public override bool CanConvert(Type objectType)
    //{
    //    return typeof(IDictionary<string, SerializedPropertyInfo>).IsAssignableFrom(objectType);
    //}

    //public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    //{
    //    IDictionary<string, SerializedPropertyInfo> dictionary = existingValue as IDictionary<string, SerializedPropertyInfo> ?? new Dictionary<string, SerializedPropertyInfo>();

    //    reader.Read();

    //    while (reader.TokenType == JsonToken.PropertyName)
    //    {
    //        string name = (string)reader.Value;
    //        reader.Read();

    //        SerializedPropertyInfo value = SerializedPropertyInfoConverter.Read(reader);
    //        reader.Read();

    //        dictionary[name] = value;
    //    }

    //    return dictionary;
    //}

    //public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    //{
    //    writer.WriteStartObject();

    //    var propertyDictionary = (IDictionary<string, SerializedPropertyInfo>)value;
    //    foreach (KeyValuePair<string, SerializedPropertyInfo> pair in propertyDictionary)
    //    {
    //        writer.WritePropertyName(pair.Key);
    //        SerializedPropertyInfoConverter.Write(writer, pair.Value);
    //    }

    //    writer.WriteEndObject();
    //}
    public override IDictionary<string, SerializedPropertyInfo>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        Dictionary<string, SerializedPropertyInfo> dictionary = new();

        reader.Read();

        while (reader.TokenType != JsonTokenType.EndObject)
        {
            string name = reader.GetString()!;
            reader.Read();

            SerializedPropertyInfo value = Instance.Read(ref reader, typeof(SerializedPropertyInfo), options)!;
            reader.Read();

            dictionary[name] = value;
        }

        return dictionary;
    }

    public override void Write(Utf8JsonWriter writer, IDictionary<string, SerializedPropertyInfo> value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}

internal class SerializedPropertyInfoConverter : System.Text.Json.Serialization.JsonConverter<SerializedPropertyInfo>
{
    //public static SerializedPropertyInfo Read(JsonReader reader)
    //{
    //    if (reader.TokenType == JsonToken.Null)
    //    {
    //        return null;
    //    }
    //    else
    //    {
    //        bool wasString = reader.TokenType == JsonToken.String;

    //        var builder = new StringBuilder();
    //        using (var w = new StringWriter(builder))
    //        using (var writer = new JsonTextWriter(w))
    //        {
    //            writer.WriteToken(reader);
    //        }

    //        return new SerializedPropertyInfo(builder.ToString(), wasString);
    //    }
    //}

    //public static void Write(JsonWriter writer, SerializedPropertyInfo value)
    //{
    //    SerializedPropertyInfo spi = value;

    //    if (spi == null || spi.SerializedValue == null)
    //    {
    //        writer.WriteNull();
    //    }
    //    else
    //    {
    //        writer.WriteRawValue(spi.SerializedValue);
    //    }
    //}

    //public override bool CanConvert(Type objectType)
    //{
    //    return objectType == typeof(SerializedPropertyInfo);
    //}

    //public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    //{
    //    return Read(reader);
    //}

    //public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    //{
    //    Write(writer, (SerializedPropertyInfo)value);
    //}
    public override SerializedPropertyInfo? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            double d = reader.GetDouble();
            return new SerializedPropertyInfo(System.Text.Json.JsonSerializer.Serialize(d), isString: false);
        }

        if (reader.TokenType == JsonTokenType.StartArray)
        {
            string arr = JsonExtensions.ToRawString(ref reader);
            return new SerializedPropertyInfo(System.Text.Json.JsonSerializer.Serialize(arr), isString: false);
        }

        if (reader.TokenType == JsonTokenType.StartObject)
        {
            string arr = JsonExtensions.ToRawString(ref reader);
            return new SerializedPropertyInfo(System.Text.Json.JsonSerializer.Serialize(arr), isString: false);
        }

        string? value = reader.GetString();

        if (value is null)
        {
            return null;
        }

        bool wasString = reader.TokenType == JsonTokenType.String;

        return new SerializedPropertyInfo(System.Text.Json.JsonSerializer.Serialize(value), wasString);
    }

    public override void Write(Utf8JsonWriter writer, SerializedPropertyInfo value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}

internal class SpyConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return false;
    }

    public override System.Text.Json.Serialization.JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}

internal class EnumConverterFactory : JsonConverterFactory
{
    private static readonly List<string> LegalTwoLetterWordsList = ["in"];

    public override bool CanConvert(Type typeToConvert)
    {
        if (typeToConvert.IsEnum)
        {
            return true;
        }

        return false;
    }

    public override System.Text.Json.Serialization.JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        //Type[] typeArguments = type.GetGenericArguments();
        //Type keyType = typeArguments[0];
        //Type valueType = typeArguments[1];

        System.Text.Json.Serialization.JsonConverter converter = (System.Text.Json.Serialization.JsonConverter)Activator.CreateInstance(
            typeof(EnumConverter<>).MakeGenericType(typeToConvert),
            BindingFlags.Instance | BindingFlags.Public,
            binder: null,
            args: [options],
            culture: null)!;

        return converter;
    }

    private sealed class EnumConverter<TEnum> : System.Text.Json.Serialization.JsonConverter<TEnum> where TEnum : struct, Enum
    {
        public EnumConverter(JsonSerializerOptions options)
        {
        }

        public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return (TEnum)Enum.Parse(typeof(TEnum), ConvertToPascalCase(reader.GetString()!));
        }

        public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        internal static string ConvertToPascalCase(string camelCaseName)
        {
            if (camelCaseName.Length == 1)
            {
                return camelCaseName.ToUpperInvariant();
            }

            int prefixCount = IsPrefixedWithTwoLetterAbbreviation(camelCaseName) ? 2 : 1;

            return camelCaseName.Substring(0, prefixCount).ToUpperInvariant() + camelCaseName.Substring(prefixCount);
        }

        private static bool IsPrefixedWithTwoLetterAbbreviation(string name)
        {
            if (name.Length < 2)
            {
                return false;
            }

            if (LegalTwoLetterWordsList.Contains(name.Substring(0, 2)))
            {
                return false;
            }

            bool isPrefixedWithTwoLetterWord = char.IsUpper(name[0]) == char.IsUpper(name[1]);

            if (name.Length == 2)
            {
                return isPrefixedWithTwoLetterWord;
            }

            return char.IsDigit(name[2]) || char.IsUpper(name[2]);
        }
    }
}

    internal class SarifVersionConverter : System.Text.Json.Serialization.JsonConverter<SarifVersion>
{
    public override SarifVersion Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetString().ConvertToSarifVersion();
    }

    public override void Write(Utf8JsonWriter writer, SarifVersion dateTimeValue, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}

internal class DataContractResolver : DefaultJsonTypeInfoResolver
{
    [DataContract]
    public class TestClass
    {
        [System.Text.Json.Serialization.JsonIgnore] // ignored by the custom resolver 
        [DataMember(Name = "stringValue", Order = 2)]
        public string String { get; set; }

        [JsonPropertyName("BOOL_VALUE")] // ignored by the custom resolver 
        [DataMember(Name = "boolValue", Order = 1)]
        public bool Boolean { get; set; }

        [JsonPropertyOrder(int.MaxValue)] // ignored by the custom resolver 
        [DataMember(Name = "intValue", Order = 0)]
        public int Int { get; set; }

        [IgnoreDataMember]
        public string Ignored { get; set; }
    }

    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        JsonTypeInfo jsonTypeInfo = base.GetTypeInfo(type, options);

        if (jsonTypeInfo.Kind == JsonTypeInfoKind.Object &&
            type.GetCustomAttribute<DataContractAttribute>() is not null)
        {
            jsonTypeInfo.Properties.Clear();

            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
            foreach (PropertyInfo propInfo in properties)
            {
                if (propInfo.GetCustomAttribute<IgnoreDataMemberAttribute>() is not null)
                {
                    continue;
                }

                if (propInfo.GetCustomAttribute<Newtonsoft.Json.JsonIgnoreAttribute>() is not null)
                {
                    continue;
                }

                if (propInfo.PropertyType.IsAssignableTo(typeof(IDictionary<string, SerializedPropertyInfo>)))
                {
                    _ = 2;
                }

                DataMemberAttribute? attr = propInfo.GetCustomAttribute<DataMemberAttribute>();
                JsonPropertyInfo jsonPropertyInfo = jsonTypeInfo.CreateJsonPropertyInfo(propInfo.PropertyType, attr?.Name ?? propInfo.Name);
                jsonPropertyInfo.Order = attr?.Order ?? 0;
                jsonPropertyInfo.Get =
                    propInfo.CanRead
                    ? propInfo.GetValue
                    : null;

                jsonPropertyInfo.Set = propInfo.CanWrite
                    ? propInfo.SetValue
                    : null;

                jsonTypeInfo.Properties.Add(jsonPropertyInfo);
            }
        }

        return jsonTypeInfo;
    }
}

internal static class JsonExtensions
{
    public static string ToRawString(ref Utf8JsonReader reader)
    {
        using (var jsonDoc = JsonDocument.ParseValue(ref reader))
        {
            return jsonDoc.RootElement.GetRawText();
        }
    }

    public static Action<JsonTypeInfo> RemapNames(Type type, IEnumerable<KeyValuePair<string, string>> names)
    {
        // Snapshot the incoming map
        var dictionary = names.ToDictionary(p => p.Key, p => p.Value).AsReadOnly();
        return typeInfo =>
        {
            if (typeInfo.Kind != JsonTypeInfoKind.Object || !type.IsAssignableFrom(typeInfo.Type))
                return;
            foreach (var property in typeInfo.Properties)
                if (property.GetMemberName() is { } memberName && dictionary.TryGetValue(memberName, out var jsonName))
                    property.Name = jsonName;
        };
    }

    public static string? GetMemberName(this JsonPropertyInfo property) => (property.AttributeProvider as MemberInfo)?.Name;
}

/// <summary>
/// Parses SARIF logs to extract the diagnostic configurations.
/// </summary>
public class SarifParser
{
    private static readonly Version MinimumCompilerVersion = new(4, 8, 0);
    private const int MaxDiagnosticSeverities = 4; // Keep in-sync with the value of `Enum.GetValues(typeof(DiagnosticSeverity)).Length`
    private static readonly JsonSerializerOptions Options = new()
    {
        TypeInfoResolver = new DataContractResolver
        {
        },
        Converters =
        {
            new SarifVersionConverter(),
            new EnumConverterFactory(),
            new SerializedPropertyInfoConverter(),
            new PropertyBagConverter(),
            //new SpyConverterFactory(),
        },
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
    };

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
            //log = SarifLog.Load(stream, deferred: true);
            log = System.Text.Json.JsonSerializer.Deserialize<SarifLog>(stream, Options)!;
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
