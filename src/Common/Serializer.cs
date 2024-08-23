using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.EventEmitters;
using YamlDotNet.Core.Events;
using YamlDotNet.Core;
using YamlDotNet.Serialization.TypeInspectors;
using YamlDotNet.Serialization.ObjectFactories;
using YamlDotNet.Serialization.ObjectGraphTraversalStrategies;

namespace SquiggleCop.Common;

/// <summary>
/// Encapsulates the formatting for serializing baselines.
/// </summary>
public class Serializer
{
    private readonly ISerializer _serializer;

    /// <summary>
    /// Creates a new instance of the <see cref="Serializer"/> class.
    /// </summary>
    public Serializer()
    {
        INamingConvention namingConvention = PascalCaseNamingConvention.Instance;

        _serializer = new SerializerBuilder()
            .WithNamingConvention(namingConvention)
            .DisableAliases() // We don't use aliases, so disable checking to improve performance -- https://github.com/aaubry/YamlDotNet/wiki/Serialization.Serializer#disablealiases
            .WithEventEmitter(next => new FlowEverythingEmitter(next)) // Use YAML flow so that rule diffs are always a single line
            .WithNewLine("\n") // Normalize newlines to prevent diff churn

            // BEGIN PERF optimizations; see //src/Common/YamlDotNet/README.md for more details.
            .WithTypeInspector(inner => new FastTypeInspector())
            .WithObjectGraphTraversalStrategyFactory((typeInspector, typeResolver, typeConverters, maxRecursion) => new FastObjectGraphTraversalStrategy(typeInspector, typeResolver, maxRecursion, namingConvention, new FastObjectFactory()))
            // END PERF

        .Build();
    }

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

    /// <summary>
    /// <see cref="IEventEmitter"/> that enables "flow" style for everything that supports it
    /// *except* for the top-level collection of <see cref="DiagnosticConfig"/>, otherwise the entire
    /// file would be a single line.
    /// </summary>
    /// <remarks>
    /// From https://github.com/aaubry/YamlDotNet/issues/454.
    /// </remarks>
    private sealed class FlowEverythingEmitter : ChainedEventEmitter
    {
        public FlowEverythingEmitter(IEventEmitter nextEmitter) : base(nextEmitter) { }

        public override void Emit(MappingStartEventInfo eventInfo, IEmitter emitter)
        {
            eventInfo.Style = MappingStyle.Flow;
            base.Emit(eventInfo, emitter);
        }

        public override void Emit(SequenceStartEventInfo eventInfo, IEmitter emitter)
        {
            // Don't flow the top-level object or else the entire baseline will be a single line
            if (!typeof(IEnumerable<DiagnosticConfig>).IsAssignableFrom(eventInfo.Source.Type))
            {
                eventInfo.Style = SequenceStyle.Flow;
            }

            nextEmitter.Emit(eventInfo, emitter);
        }
    }
}
