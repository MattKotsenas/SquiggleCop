using BenchmarkDotNet.Attributes;

using SquiggleCop.Common;
using SquiggleCop.Common.Tests;

namespace SquiggleCop.Benchmarks;

[Config(typeof(Config))]
public class Serializing
{
    private static readonly TestDataReader Reader = new(typeof(Program).Namespace!);

    private readonly IReadOnlyCollection<DiagnosticConfig> _configs = CreateConfigs();
    private readonly Serializer _serializer = new();

    private static IReadOnlyCollection<DiagnosticConfig> CreateConfigs()
    {
        using Stream file = Reader.Read("Log.v2.sarif");

        return new SarifParser().Parse(file);
    }

    [Benchmark]
    public string Serialize()
    {
        return _serializer.Serialize(_configs);
    }
}
