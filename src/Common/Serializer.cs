using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;

namespace SquiggleCop.Common;

/// <summary>
/// Encapsulates the formatting for serializing baselines.
/// </summary>
public class Serializer
{
    private readonly ISerializer _serializer = new SerializerBuilder()
    .WithNamingConvention(PascalCaseNamingConvention.Instance)
    .Build();

    // TODO: Consider using a TextWriter or something for better perf.
    /// <summary>
    /// Serializes the given value to a string.
    /// </summary>
    /// <typeparam name="T">The type of object or object graph to serialize.</typeparam>
    /// <param name="value">The object or object graph to serialize.</param>
    /// <returns>
    /// The string representation of the object or object graph.
    /// </returns>
    public string Serialize<T>(T value)
    {
        return _serializer.Serialize(value);
    }
}
