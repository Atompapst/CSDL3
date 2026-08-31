extern alias RealCSDL;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using FRect = RealCSDL::CSDL.Video.FRect;

namespace GenericBenchmark.Benchmarks {
    /// <summary>
    ///     Managed CSDL_IMPL port (<see cref="FRect.GetRectIntersection(FRect, FRect, out FRect)"/>,
    ///     CSDL/Video/rect/FRect.Extension.cs) vs the native SDL_GetRectIntersectionFloat it stands in for.
    /// </summary>
    [MediumRunJob]
    [MemoryDiagnoser]
    public unsafe partial class FRectGetIntersectionBenchmarks : ManagedVsNativeBenchmarks<bool> {
        private static readonly FRect B = new FRect(50f, 50f, 100f, 100f);

        // Mutated per call (like RectHasIntersectionBenchmarks) so the JIT can't prove the input is
        // invariant across the unrolled loop and constant-fold the whole call away.
        private int _counter;

        [GlobalSetup]
        public void Setup() {
            ValidateManagedMatchesNative();

            FRect a = new FRect(0f, 0f, 100f, 100f);
            FRect.GetRectIntersection(a, B, out FRect managed);
            NativeFRect na = new NativeFRect(a);
            NativeFRect nb = new NativeFRect(B);
            GetRectIntersectionFloat(&na, &nb, out NativeFRect native);
            if (managed.X != native.X || managed.Y != native.Y || managed.W != native.W || managed.H != native.H) {
                throw new InvalidOperationException("Managed result rect differs from native result rect.");
            }

            _counter = 0;
        }

        [Benchmark(Baseline = true)]
        public override bool Managed() {
            FRect a = new FRect(_counter++ & 15, 0f, 100f, 100f);
            return FRect.GetRectIntersection(a, B, out _);
        }

        [Benchmark]
        public override bool Native() {
            NativeFRect a = new NativeFRect(new FRect(_counter++ & 15, 0f, 100f, 100f));
            NativeFRect b = new NativeFRect(B);
            return GetRectIntersectionFloat(&a, &b, out _) != 0;
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

        [LibraryImport("SDL3", EntryPoint = "SDL_GetRectIntersectionFloat")]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        private static partial byte GetRectIntersectionFloat(NativeFRect* a, NativeFRect* b, out NativeFRect result);
    }
}
