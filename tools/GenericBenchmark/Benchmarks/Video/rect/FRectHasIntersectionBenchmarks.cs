extern alias RealCSDL;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using FRect = RealCSDL::CSDL.Video.FRect;

namespace GenericBenchmark.Benchmarks {
    /// <summary>
    ///     Managed CSDL_IMPL port (<see cref="FRect.Intersects(FRect, FRect)"/>, CSDL/Video/rect/FRect.CSDL_IMPL.cs)
    ///     vs the native SDL_HasRectIntersectionFloat it stands in for.
    /// </summary>
    [MediumRunJob]
    [MemoryDiagnoser]
    public unsafe partial class FRectHasIntersectionBenchmarks : ManagedVsNativeBenchmarks<bool> {
        private static readonly FRect B = new FRect(50f, 50f, 100f, 100f);

        // Mutated per call (like RectHasIntersectionBenchmarks) so the JIT can't prove the input is
        // invariant across the unrolled loop and constant-fold the whole call away.
        private int _counter;

        [GlobalSetup]
        public void Setup() {
            ValidateManagedMatchesNative();
        }

        [Benchmark(Baseline = true)]
        public override bool Managed() {
            FRect a = new FRect(_counter++ & 15, 0f, 100f, 100f);
            return FRect.Intersects(a, B);
        }

        [Benchmark]
        public override bool Native() {
            NativeFRect a = new NativeFRect(new FRect(_counter++ & 15, 0f, 100f, 100f));
            NativeFRect b = new NativeFRect(B);
            return HasRectIntersectionFloat(&a, &b) != 0;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct NativeFRect {
            public float X, Y, W, H;

            public NativeFRect(FRect rect) {
                X = rect.X;
                Y = rect.Y;
                W = rect.W;
                H = rect.H;
            }
        }

        [LibraryImport("SDL3", EntryPoint = "SDL_HasRectIntersectionFloat")]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        private static partial byte HasRectIntersectionFloat(NativeFRect* a, NativeFRect* b);
    }
}
