extern alias RealCSDL;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Rect = RealCSDL::CSDL.Video.Rect;

namespace GenericBenchmark.Benchmarks;

/// <summary>
///     Managed CSDL_IMPL port (<see cref="Rect.GetRectAndLineIntersection"/>, CSDL/Video/rect/Rect.Extension.cs)
///     vs the native SDL_GetRectAndLineIntersection it stands in for. The line runs diagonally through
///     the rect (x1 != x2 and y1 != y2) so both implementations exercise the full Cohen-Sutherland clip
///     loop rather than the horizontal/vertical fast paths.
/// </summary>
[MediumRunJob]
[MemoryDiagnoser]
public unsafe partial class RectGetAndLineIntersectionBenchmarks : ManagedVsNativeBenchmarks<bool> {
    private static readonly Rect Bounds = new(50, 50, 100, 100);

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
        int x1 = -100 + (_counter++ & 15), y1 = -100, x2 = 300, y2 = 300;
        return Rect.GetRectAndLineIntersection(in Bounds, ref x1, ref y1, ref x2, ref y2);
    }

    [Benchmark]
    public override bool Native() {
        NativeRect bounds = new(Bounds);
        int x1 = -100 + (_counter++ & 15), y1 = -100, x2 = 300, y2 = 300;
        return GetRectAndLineIntersection(&bounds, ref x1, ref y1, ref x2, ref y2) != 0;
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

    [LibraryImport("SDL3", EntryPoint = "SDL_GetRectAndLineIntersection")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte GetRectAndLineIntersection(NativeRect* rect, ref int x1, ref int y1, ref int x2, ref int y2);
}
