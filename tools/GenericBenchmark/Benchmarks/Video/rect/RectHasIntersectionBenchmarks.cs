extern alias RealCSDL;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using Rect = RealCSDL::CSDL.Video.Rect;

namespace GenericBenchmark.Benchmarks {
    /// <summary>
    ///     Managed CSDL_IMPL port (<see cref="Rect.HasIntersection(in Rect)"/>, CSDL/Video/rect/Rect.CSDL_IMPL.cs)
    ///     vs the native SDL_HasRectIntersection it stands in for. Confirms the managed port is worth
    ///     it for this call shape (two small structs, almost no native work) rather than assuming it.
    /// </summary>
    [MediumRunJob]
    [MemoryDiagnoser]
    public unsafe partial class RectHasIntersectionBenchmarks : ManagedVsNativeBenchmarks<bool> {
        private static readonly Rect B = new Rect(50, 50, 100, 100);

        // Mutated per call (like the original SDL probe's `a.x = i & 15`) so the JIT can't prove the
        // input is invariant across the unrolled loop and constant-fold the whole call away.
        private int _counter;

        [GlobalSetup]
        public void Setup() {
            ValidateManagedMatchesNative();
        }

        [Benchmark(Baseline = true)]
        public override bool Managed() {
            Rect a = new Rect(_counter++ & 15, 0, 100, 100);
            return Rect.HasIntersection(in a, in B);
        }

        [Benchmark]
        public override bool Native() {
            NativeRect a = new NativeRect(new Rect(_counter++ & 15, 0, 100, 100));
            NativeRect b = new NativeRect(B);
            return HasRectIntersection(&a, &b) != 0;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct NativeRect {
            public int X, Y, W, H;

            public NativeRect(Rect rect) {
                X = rect.X;
                Y = rect.Y;
                W = rect.W;
                H = rect.H;
            }
        }

        [LibraryImport("SDL3", EntryPoint = "SDL_HasRectIntersection")]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        private static partial byte HasRectIntersection(NativeRect* a, NativeRect* b);
    }
}
