using System.Reflection;
using BenchmarkDotNet.Running;

// ==================== start parameters ====================
// Every method that gets benchmarked, its variants, setup/cleanup, [Params] sweeps is
// defined by the [Benchmark] classes under Benchmarks/, and BenchmarkSwitcher
// discovers them automatically.
//
// Usage:
//   dotnet run -c Release --project tools/GenericBenchmark (interactive picker)
//   dotnet run -c Release --project tools/GenericBenchmark -- --filter *ExampleBenchmarks*
BenchmarkSwitcher.FromAssembly(Assembly.GetExecutingAssembly()).Run(args);
