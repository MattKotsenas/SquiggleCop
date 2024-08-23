using BenchmarkDotNet.Attributes;

using SquiggleCop.Common;
using SquiggleCop.Common.Tests;

namespace SquiggleCop.Benchmarks;

[Config(typeof(Config))]
public class Diffing
{
    private static readonly TestDataReader Reader = new(typeof(Diffing).Namespace!);

    private readonly BaselineDiffer _differ = new();
    private readonly string _originalBaseline = CreateOriginalBaseline();
    private readonly string _newBaseline = CreateNewBaseline();

    private static string CreateOriginalBaseline()
    {
        using Stream file = Reader.Read("SquiggleCop.v2.Baseline.yaml");

        return new StreamReader(file).ReadToEnd();
    }

    private static string CreateNewBaseline()
    {
        using Stream file = Reader.Read("Log.v2.sarif");

        SarifParser parser = new();
        Serializer serializer = new();

        return serializer.Serialize(parser.Parse(file));
    }

    [Benchmark]
    public bool Same()
    {
        return _differ.Diff(_originalBaseline, _newBaseline).HasDifferences;
    }

    [Benchmark]
    public bool DifferenceAtEnd()
    {
        string baseline = _originalBaseline + "extra";
        return _differ.Diff(baseline, _newBaseline).HasDifferences;
    }

    [Benchmark]
    public bool DifferenceAtStart()
    {
        string baseline = "extra" + _originalBaseline;
        return _differ.Diff(baseline, _newBaseline).HasDifferences;
    }
}
