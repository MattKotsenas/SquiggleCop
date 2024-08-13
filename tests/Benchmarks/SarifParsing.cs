using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;

using SquiggleCop.Common;
using SquiggleCop.Common.Tests;

namespace SquiggleCop.Benchmarks;

[Config(typeof(Config))]
public class SarifParsing
{
    private static readonly TestDataReader Reader = new(typeof(Program).Namespace!);

    private Stream _stream = null!;
    private readonly SarifParser _parser = new();
    private readonly Consumer _consumer = new();

    [IterationSetup]
    public void CreateSarifStream()
    {
        using Stream file = Reader.Read("Log.v2.sarif");
        MemoryStream memory = new();

        file.CopyTo(memory);
        memory.Position = 0;

        _stream = memory;
    }

    [Benchmark]
    public void Parse()
    {
        _parser.Parse(_stream).Consume(_consumer);
    }

    [Benchmark]
    public async Task ParseAsync()
    {
        IEnumerable<DiagnosticConfig> configs = await _parser.ParseAsync(_stream).ConfigureAwait(false);
        configs.Consume(_consumer);
    }
}
