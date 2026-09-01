extern alias RealCSDL;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using FRect = RealCSDL::CSDL.Video.FRect;

namespace GenericBenchmark.Benchmarks {
    /// <summary>
    ///     Managed CSDL_IMPL port (<see cref="FRect.GetRectAndLineIntersection"/>, CSDL/Video/rect/FRect.CSDL_IMPL.cs)
    ///     vs the native SDL_GetRectAndLineIntersectionFloat it stands in for. The line runs diagonally
    ///     through the rect (x1 != x2 and y1 != y2) so both implementations exercise the full
    ///     Cohen-Sutherland clip loop rather than the horizontal/vertical fast paths.
    /// </summary>
    [MediumRunJob]
    [MemoryDiagnoser]
    public unsafe partial class FRectGetAndLineIntersectionBenchmarks : ManagedVsNativeBenchmarks<bool> {
        private static readonly FRect Bounds = new FRect(50f, 50f, 100f, 100f);

        // Mutated per call (like RectHasIntersectionBenchmarks) so the JIT can't prove the input is
        // invariant across the unrolled loop and constant-fold the whole call away.
        private int _counter;

        [GlobalSetup]
        public void Setup() {
            ValidateManagedMatchesNative();
            _counter = 0;
        }

        [Benchmark(Baseline = true)]
        public override bool Managed() {
            float x1 = -100f + (_counter++ & 15), y1 = -100f, x2 = 300f, y2 = 300f;
            return FRect.GetRectAndLineIntersection(Bounds, ref x1, ref y1, ref x2, ref y2);
        }

        [Benchmark]
        public override bool Native() {
            NativeFRect bounds = new NativeFRect(Bounds);
            float x1 = -100f + (_counter++ & 15), y1 = -100f, x2 = 300f, y2 = 300f;
            return GetRectAndLineIntersectionFloat(&bounds, ref x1, ref y1, ref x2, ref y2) != 0;
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

        [LibraryImport("SDL3", EntryPoint = "SDL_GetRectAndLineIntersectionFloat")]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        private static partial byte GetRectAndLineIntersectionFloat(NativeFRect* rect, ref float x1, ref float y1, ref float x2, ref float y2);
    }
}
