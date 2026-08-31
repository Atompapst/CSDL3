extern alias RealCSDL;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Rect = RealCSDL::CSDL.Video.Rect;

namespace GenericBenchmark.Benchmarks {
    [MediumRunJob]
    [MemoryDiagnoser]
    public unsafe partial class RectIntersectionCallingConventionBenchmarks {
        private Rect _b = new Rect(50, 50, 100, 100);

        // Mutated per call (like RectHasIntersectionBenchmarks) so the JIT can't prove the input is
        // invariant across the unrolled loop and constant-fold the whole call away.
        private int _counter;

        [GlobalSetup]
        public void Setup() {
            Rect a = new Rect(0, 0, 100, 100);
            bool expected = Native();
            _counter = 0;

            if (Production() != expected) {
                throw new InvalidOperationException("Production result differs from native.");
            }
            if (IntersectsIn(in a, in _b) != expected) {
                throw new InvalidOperationException("ManagedIn result differs from native.");
            }
            if (IntersectsRef(ref a, ref _b) != expected) {
                throw new InvalidOperationException("ManagedRef result differs from native.");
            }

            Rect* pa = &a;
            fixed (Rect* pb = &_b) {
                if (IntersectsPointer(pa, pb) != expected) {
                    throw new InvalidOperationException("ManagedPointer result differs from native.");
                }
            }

            _counter = 0;
        }

        [Benchmark(Baseline = true)]
        public bool Production() {
            Rect a = new Rect(_counter++ & 15, 0, 100, 100);
            return Rect.HasIntersection(in a, in _b);
        }

        [Benchmark]
        public bool Native() {
            NativeRect a = new NativeRect(new Rect(_counter++ & 15, 0, 100, 100));
            NativeRect b = new NativeRect(_b);
            return HasRectIntersection(&a, &b) != 0;
        }

        [Benchmark]
        public bool ManagedIn() {
            Rect a = new Rect(_counter++ & 15, 0, 100, 100);
            return IntersectsIn(in a, in _b);
        }

        [Benchmark]
        public bool ManagedRef() {
            Rect a = new Rect(_counter++ & 15, 0, 100, 100);
            return IntersectsRef(ref a, ref _b);
        }

        [Benchmark]
        public bool ManagedPointer() {
            Rect a = new Rect(_counter++ & 15, 0, 100, 100);
            Rect* pa = &a;
            fixed (Rect* pb = &_b) {
                return IntersectsPointer(pa, pb);
            }
        }

        // Current style: in Rect == readonly byref. Matches Rect.HasIntersection's actual signature.
        private static bool IntersectsIn(in Rect a, in Rect b) {
            if (RectCanOverflowIn(in a) || RectCanOverflowIn(in b)) {
                return false;
            }

            int aMin = a.X;
            int aMax = aMin + a.W;
            int bMin = b.X;
            int bMax = bMin + b.W;
            if (bMin > aMin) {
                aMin = bMin;
            }
            if (bMax < aMax) {
                aMax = bMax;
            }
            if (aMax - 1 < aMin) {
                return false;
            }

            aMin = a.Y;
            aMax = aMin + a.H;
            bMin = b.Y;
            bMax = bMin + b.H;
            if (bMin > aMin) {
                aMin = bMin;
            }
            if (bMax < aMax) {
                aMax = bMax;
            }
            if (aMax - 1 < aMin) {
                return false;
            }

            return true;
        }

        // ref Rect: writable byref. Interesting for the benchmark even though the method never writes.
        private static bool IntersectsRef(ref Rect a, ref Rect b) {
            if (RectCanOverflowRef(ref a) || RectCanOverflowRef(ref b)) {
                return false;
            }

            int aMin = a.X;
            int aMax = aMin + a.W;
            int bMin = b.X;
            int bMax = bMin + b.W;
            if (bMin > aMin) {
                aMin = bMin;
            }
            if (bMax < aMax) {
                aMax = bMax;
            }
            if (aMax - 1 < aMin) {
                return false;
            }

            aMin = a.Y;
            aMax = aMin + a.H;
            bMin = b.Y;
            bMax = bMin + b.H;
            if (bMin > aMin) {
                aMin = bMin;
            }
            if (bMax < aMax) {
                aMax = bMax;
            }
            if (aMax - 1 < aMin) {
                return false;
            }

            return true;
        }

        private static bool IntersectsPointer(Rect* a, Rect* b) {
            if (RectCanOverflowPointer(a) || RectCanOverflowPointer(b)) {
                return false;
            }

            int aMin = a->X;
            int aMax = aMin + a->W;
            int bMin = b->X;
            int bMax = bMin + b->W;
            if (bMin > aMin) {
                aMin = bMin;
            }
            if (bMax < aMax) {
                aMax = bMax;
            }
            if (aMax - 1 < aMin) {
                return false;
            }

            aMin = a->Y;
            aMax = aMin + a->H;
            bMin = b->Y;
            bMax = bMin + b->H;
            if (bMin > aMin) {
                aMin = bMin;
            }
            if (bMax < aMax) {
                aMax = bMax;
            }
            if (aMax - 1 < aMin) {
                return false;
            }

            return true;
        }

        private static bool RectCanOverflowIn(in Rect r) {
            const int halfMax = int.MaxValue / 2;
            const int halfMin = int.MinValue / 2;

            return r.X <= halfMin || r.X >= halfMax ||
                   r.Y <= halfMin || r.Y >= halfMax ||
                   r.W >= halfMax || r.H >= halfMax;
        }

        private static bool RectCanOverflowRef(ref Rect r) {
            const int halfMax = int.MaxValue / 2;
            const int halfMin = int.MinValue / 2;

            return r.X <= halfMin || r.X >= halfMax ||
                   r.Y <= halfMin || r.Y >= halfMax ||
                   r.W >= halfMax || r.H >= halfMax;
        }

        private static bool RectCanOverflowPointer(Rect* r) {
            const int halfMax = int.MaxValue / 2;
            const int halfMin = int.MinValue / 2;

            return r->X <= halfMin || r->X >= halfMax ||
                   r->Y <= halfMin || r->Y >= halfMax ||
                   r->W >= halfMax || r->H >= halfMax;
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
