using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess.Emit;

namespace SquiggleCop.Benchmarks;

public class Config : ManualConfig
{
    public Config()
    {
        AddDiagnoser(MemoryDiagnoser.Default);
        AddDiagnoser(ExceptionDiagnoser.Default);

        AddJob(Job
            .ShortRun
            .WithToolchain(InProcessEmitToolchain.Instance)
            .WithRuntime(CoreRuntime.Core80));

        AddJob(Job
            .ShortRun
            .WithToolchain(InProcessEmitToolchain.Instance)
            .WithRuntime(CoreRuntime.Core60));

        AddJob(Job
            .ShortRun
            .WithToolchain(InProcessEmitToolchain.Instance)
            .WithRuntime(ClrRuntime.Net472));
    }
}
