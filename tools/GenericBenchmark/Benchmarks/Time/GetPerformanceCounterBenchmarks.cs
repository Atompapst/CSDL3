extern alias RealCSDL;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using Timer = RealCSDL::CSDL.Timer;

namespace GenericBenchmark.Benchmarks.Time {
    [MediumRunJob]
    [MemoryDiagnoser]
    public partial class GetPerformanceCounterBenchmarks : ManagedVsNativeBenchmarks<ulong> {
        [GlobalSetup]
        public void Setup() {
            ulong managedFreq = Timer.GetPerformanceFrequency();
            ulong nativeFreq = GetPerformanceFrequency();

            ulong managedStart = Timer.GetPerformanceCounter();
            ulong nativeStart = GetPerformanceCounter();
            System.Threading.Thread.Sleep(20);
            ulong managedEnd = Timer.GetPerformanceCounter();
            ulong nativeEnd = GetPerformanceCounter();

            double managedElapsedMs = (managedEnd - managedStart) * 1000.0 / managedFreq;
            double nativeElapsedMs = (nativeEnd - nativeStart) * 1000.0 / nativeFreq;
            double diffMs = Math.Abs(managedElapsedMs - nativeElapsedMs);
            if (diffMs > 5) {
                throw new InvalidOperationException($"GetPerformanceCounter: managed elapsed ({managedElapsedMs:F3}ms) and native elapsed ({nativeElapsedMs:F3}ms) over the same ~20ms sleep disagree by {diffMs:F3}ms.");
            }
        }

        [Benchmark(Baseline = true)]
        public override ulong Managed() {
            return Timer.GetPerformanceCounter();
        }

        [Benchmark]
        public override ulong Native() {
            return GetPerformanceCounter();
        }

        [LibraryImport("SDL3", EntryPoint = "SDL_GetPerformanceCounter")]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        private static partial ulong GetPerformanceCounter();

        [LibraryImport("SDL3", EntryPoint = "SDL_GetPerformanceFrequency")]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        private static partial ulong GetPerformanceFrequency();
    }
}
