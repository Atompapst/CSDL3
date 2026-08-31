extern alias RealCSDL;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using Rect = RealCSDL::CSDL.Video.Rect;

namespace GenericBenchmark.Benchmarks {
    /// <summary>
    ///     Managed CSDL_IMPL port (<see cref="Rect.TryUnion(Rect, Rect, out Rect)"/>,
    ///     CSDL/Video/rect/Rect.Extension.cs) vs the native SDL_GetRectUnion it stands in for.
    /// </summary>
    [MediumRunJob]
    [MemoryDiagnoser]
    public unsafe partial class RectGetUnionBenchmarks : ManagedVsNativeBenchmarks<bool> {
        private static readonly Rect B = new Rect(50, 50, 100, 100);

        // Mutated per call (like RectHasIntersectionBenchmarks) so the JIT can't prove the input is
        // invariant across the unrolled loop and constant-fold the whole call away.
        private int _counter;

        [GlobalSetup]
        public void Setup() {
            ValidateManagedMatchesNative();

            Rect a = new Rect(0, 0, 100, 100);
            Rect.TryUnion(a, B, out Rect managed);
            NativeRect na = new NativeRect(a);
            NativeRect nb = new NativeRect(B);
            GetRectUnion(&na, &nb, out NativeRect native);
            if (managed.X != native.X || managed.Y != native.Y || managed.W != native.W || managed.H != native.H) {
                throw new InvalidOperationException("Managed result rect differs from native result rect.");
            }

            _counter = 0;
        }

        [Benchmark(Baseline = true)]
        public override bool Managed() {
            Rect a = new Rect(_counter++ & 15, 0, 100, 100);
            return Rect.TryUnion(a, B, out _);
        }

        [Benchmark]
        public override bool Native() {
            NativeRect a = new NativeRect(new Rect(_counter++ & 15, 0, 100, 100));
            NativeRect b = new NativeRect(B);
            return GetRectUnion(&a, &b, out _) != 0;
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

        [LibraryImport("SDL3", EntryPoint = "SDL_GetRectUnion")]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        private static partial byte GetRectUnion(NativeRect* a, NativeRect* b, out NativeRect result);
    }
}
