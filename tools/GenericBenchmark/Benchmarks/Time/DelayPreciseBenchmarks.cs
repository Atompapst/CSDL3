extern alias RealCSDL;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using Timer = RealCSDL::CSDL.Timer;

namespace GenericBenchmark.Benchmarks.Time {
    [MediumRunJob]
    public partial class DelayPreciseBenchmarks : ManagedVsNativeBenchmarks<ulong> {
        private const ulong TargetNs = 10_000_000; // 5ms - past the 1ms floor where the algorithm actually spins to a tight finish.

        [GlobalSetup]
        public void Setup() {
            // A handful of untimed calls first - the very first invocation eats JIT compilation of
            // DelayPrecise/NowNs, which would otherwise swamp a 5ms measurement.
            for (int i = 0; i < 5; i++) {
                Timer.DelayPrecise(TargetNs);
                DelayPrecise(TargetNs);
            }
            
            double managedOvershootMs = MeasureOvershootMs(Timer.DelayPrecise);
            double nativeOvershootMs = MeasureOvershootMs(DelayPrecise);
            if (managedOvershootMs is < 0 or > 2 || nativeOvershootMs is < 0 or > 2) {
                throw new InvalidOperationException($"DelayPrecise: managed overshoot {managedOvershootMs:F4}ms, native overshoot {nativeOvershootMs:F4}ms - expected both within [0, 2]ms.");
            }
        }

        private static double MeasureOvershootMs(Action<ulong> delayPrecise) {
            long t0 = Stopwatch.GetTimestamp();
            delayPrecise(TargetNs);
            long t1 = Stopwatch.GetTimestamp();
            double elapsedMs = (t1 - t0) * 1000.0 / Stopwatch.Frequency;
            return elapsedMs - TargetNs / 1_000_000.0;
        }

        [Benchmark(Baseline = true)]
        public override ulong Managed() {
            Timer.DelayPrecise(TargetNs);
            return TargetNs;
        }

        [Benchmark]
        public override ulong Native() {
            DelayPrecise(TargetNs);
            return TargetNs;
        }

        [LibraryImport("SDL3", EntryPoint = "SDL_DelayPrecise")]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        private static partial void DelayPrecise(ulong ns);
    }
}
