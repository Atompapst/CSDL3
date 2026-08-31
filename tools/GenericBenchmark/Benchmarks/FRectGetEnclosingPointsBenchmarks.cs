extern alias RealCSDL;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using FPoint = RealCSDL::CSDL.Video.FPoint;
using FRect = RealCSDL::CSDL.Video.FRect;

namespace GenericBenchmark.Benchmarks {
    /// <summary>
    ///     Managed CSDL_IMPL port (<see cref="FRect.TryGetEnclosingPoints"/>, CSDL/Video/rect/FRect.Extension.cs)
    ///     vs the native SDL_GetRectEnclosingPointsFloat it stands in for. Runs unclipped (clip = null),
    ///     which is the cheaper of the two code paths in both implementations.
    /// </summary>
    [MediumRunJob]
    [MemoryDiagnoser]
    public unsafe partial class FRectGetEnclosingPointsBenchmarks : ManagedVsNativeBenchmarks<bool> {
        private readonly FPoint[] _managedPoints = [new FPoint(10f, 10f), new FPoint(90f, 20f), new FPoint(40f, 80f), new FPoint(5f, 60f)];
        private readonly NativeFPoint[] _nativePoints = [new NativeFPoint(10f, 10f), new NativeFPoint(90f, 20f), new NativeFPoint(40f, 80f), new NativeFPoint(5f, 60f)];

        // Mutated per call (like RectHasIntersectionBenchmarks) so the JIT can't prove the input is
        // invariant across the unrolled loop and constant-fold the whole call away.
        private int _counter;

        [GlobalSetup]
        public void Setup() {
            ValidateManagedMatchesNative();

            FRect.TryGetEnclosingPoints(_managedPoints, null, out FRect managed);
            NativeFRect native;
            fixed (NativeFPoint* points = _nativePoints) {
                GetRectEnclosingPointsFloat(points, _nativePoints.Length, null, out native);
            }
            if (managed.X != native.X || managed.Y != native.Y || managed.W != native.W || managed.H != native.H) {
                throw new InvalidOperationException("Managed result rect differs from native result rect.");
            }

            _counter = 0;
        }

        [Benchmark(Baseline = true)]
        public override bool Managed() {
            _managedPoints[0] = new FPoint(_counter++ & 15, 10f);
            return FRect.TryGetEnclosingPoints(_managedPoints, null, out _);
        }

        [Benchmark]
        public override bool Native() {
            _nativePoints[0] = new NativeFPoint(_counter++ & 15, 10f);
            fixed (NativeFPoint* points = _nativePoints) {
                return GetRectEnclosingPointsFloat(points, _nativePoints.Length, null, out _) != 0;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct NativeFPoint {
            public float X, Y;

            public NativeFPoint(float x, float y) {
                X = x;
                Y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct NativeFRect {
            public float X, Y, W, H;
        }

        [LibraryImport("SDL3", EntryPoint = "SDL_GetRectEnclosingPointsFloat")]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        private static partial byte GetRectEnclosingPointsFloat(NativeFPoint* points, int count, NativeFRect* clip, out NativeFRect result);
    }
}
