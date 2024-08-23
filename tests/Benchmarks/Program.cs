using BenchmarkDotNet.Running;

using SquiggleCop.Benchmarks;

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);

