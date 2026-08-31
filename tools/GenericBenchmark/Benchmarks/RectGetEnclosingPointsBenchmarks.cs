extern alias RealCSDL;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Point = RealCSDL::CSDL.Video.Point;
using Rect = RealCSDL::CSDL.Video.Rect;

namespace GenericBenchmark.Benchmarks {
    /// <summary>
    ///     Managed CSDL_IMPL port (<see cref="Rect.TryGetEnclosingPoints"/>, CSDL/Video/rect/Rect.Extension.cs)
    ///     vs the native SDL_GetRectEnclosingPoints it stands in for. Runs unclipped (clip = null), which
    ///     is the cheaper of the two code paths in both implementations.
    /// </summary>
    [MediumRunJob]
    [MemoryDiagnoser]
    public unsafe partial class RectGetEnclosingPointsBenchmarks : ManagedVsNativeBenchmarks<bool> {
        private readonly Point[] _managedPoints = [new Point(10, 10), new Point(90, 20), new Point(40, 80), new Point(5, 60)];
        private readonly NativePoint[] _nativePoints = [new NativePoint(10, 10), new NativePoint(90, 20), new NativePoint(40, 80), new NativePoint(5, 60)];

        // Mutated per call (like RectHasIntersectionBenchmarks) so the JIT can't prove the input is
        // invariant across the unrolled loop and constant-fold the whole call away.
        private int _counter;

        [GlobalSetup]
        public void Setup() {
            ValidateManagedMatchesNative();

            Rect.TryGetEnclosingPoints(_managedPoints, null, out Rect managed);
            NativeRect native;
            fixed (NativePoint* points = _nativePoints) {
                GetRectEnclosingPoints(points, _nativePoints.Length, null, out native);
            }
            if (managed.X != native.X || managed.Y != native.Y || managed.W != native.W || managed.H != native.H) {
                throw new InvalidOperationException("Managed result rect differs from native result rect.");
            }

            _counter = 0;
        }

        [Benchmark(Baseline = true)]
        public override bool Managed() {
            _managedPoints[0] = new Point(_counter++ & 15, 10);
            return Rect.TryGetEnclosingPoints(_managedPoints, null, out _);
        }

        [Benchmark]
        public override bool Native() {
            _nativePoints[0] = new NativePoint(_counter++ & 15, 10);
            fixed (NativePoint* points = _nativePoints) {
                return GetRectEnclosingPoints(points, _nativePoints.Length, null, out _) != 0;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct NativePoint {
            public int X, Y;

            public NativePoint(int x, int y) {
                X = x;
                Y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct NativeRect {
            public int X, Y, W, H;
        }

        [LibraryImport("SDL3", EntryPoint = "SDL_GetRectEnclosingPoints")]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        private static partial byte GetRectEnclosingPoints(NativePoint* points, int count, NativeRect* clip, out NativeRect result);
    }
}
