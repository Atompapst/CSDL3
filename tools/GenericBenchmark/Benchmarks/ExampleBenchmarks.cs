using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace GenericBenchmark.Benchmarks {
    /// <summary>
    ///     Template: copy this class, rename it, and replace the method bodies below with whatever
    ///     you want to compare. Each [Benchmark] method is one variant; BenchmarkSwitcher finds this
    ///     class without any change to Program.cs.
    /// </summary>
    [MediumRunJob]
    [MemoryDiagnoser]
    public class ExampleBenchmarks {
        private const int Count = 100;

        [Benchmark(Baseline = true)]
        public string StringConcat() {
            string result = "";
            for (int i = 0; i < Count; i++) {
                result += i;
            }
            return result;
        }

        [Benchmark]
        public string StringBuilder() {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Count; i++) {
                sb.Append(i);
            }
            return sb.ToString();
        }
    }
}
