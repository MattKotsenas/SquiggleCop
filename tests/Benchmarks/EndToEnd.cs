using BenchmarkDotNet.Attributes;

using SquiggleCop.Common;
using SquiggleCop.Common.Tests;

namespace SquiggleCop.Benchmarks;

[Config(typeof(Config))]
public class EndToEnd
{
    private static readonly TestDataReader Reader = new(typeof(EndToEnd).Namespace!);

    private readonly BaselineDiffer _differ = new();
    private readonly SarifParser _parser = new();
    private readonly Serializer _serializer = new();

    private readonly string _originalBaseline = CreateOriginalBaseline();

    private static string CreateOriginalBaseline()
    {
        using Stream file = Reader.Read("SquiggleCop.v2.Baseline.yaml");
        return new StreamReader(file).ReadToEnd();
    }

    [Benchmark]
    public bool EndToEndPipeline()
    {
        using Stream file = Reader.Read("Log.v2.sarif");
        IReadOnlyCollection<DiagnosticConfig> configs = _parser.Parse(file);

        string baseline = _serializer.Serialize(configs);

        return _differ.Diff(_originalBaseline, baseline).HasDifferences;
    }

    [Benchmark]
    public async Task<bool> EndToEndPipelineAsync()
    {
        using Stream file = Reader.Read("Log.v2.sarif");
        IReadOnlyCollection<DiagnosticConfig> configs = await _parser.ParseAsync(file).ConfigureAwait(false);

        string baseline = _serializer.Serialize(configs);

        return _differ.Diff(_originalBaseline, baseline).HasDifferences;
    }
}
