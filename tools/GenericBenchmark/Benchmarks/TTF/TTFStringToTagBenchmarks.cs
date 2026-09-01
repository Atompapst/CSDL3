extern alias RealCSDL;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using TTF = RealCSDL::CSDL.TTF.TTF;

namespace GenericBenchmark.Benchmarks {
    /// <summary>
    ///     Managed CSDL_IMPL port (<see cref="TTF.StringToTag"/>, CSDL/TTF/TTF.CSDL_IMPL.cs) vs the
    ///     native TTF_StringToTag it stands in for. The native path also pays for LPUTF8Str marshalling
    ///     of the input string on top of the P/Invoke transition, which the managed port skips entirely.
    /// </summary>
    [MediumRunJob]
    [MemoryDiagnoser]
    public partial class TTFStringToTagBenchmarks : ManagedVsNativeBenchmarks<uint> {
        private static readonly string[] Tags = { "kern", "liga", "ss01", "DFLT", "latn", "aalt", "en", "de-DE" };

        // Rotated per call so the JIT can't prove the input is invariant and constant-fold the call
        // away. `& 7` (Tags.Length - 1, kept a power of two on purpose) instead of `% Tags.Length` -
        // a long-running job overflows _counter into negative territory, and `%` keeps the dividend's
        // sign, which would index the array out of range.
        private int _counter;

        [GlobalSetup]
        public void Setup() {
            // Not ValidateManagedMatchesNative() - Managed()/Native() each advance the shared
            // _counter, so calling them back-to-back would compare two different tags. Check every
            // sample tag explicitly instead.
            foreach (string tag in Tags) {
                uint managed = TTF.StringToTag(tag);
                uint native = StringToTag(tag);
                if (managed != native) {
                    throw new InvalidOperationException(
                        $"StringToTag('{tag}'): managed result (0x{managed:X8}) differs from native result (0x{native:X8}).");
                }
            }
        }

        [Benchmark(Baseline = true)]
        public override uint Managed() {
            return TTF.StringToTag(Tags[_counter++ & 7]);
        }

        [Benchmark]
        public override uint Native() {
            return StringToTag(Tags[_counter++ & 7]);
        }

        [LibraryImport("SDL3_ttf", EntryPoint = "TTF_StringToTag")]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        private static partial uint StringToTag([MarshalAs(UnmanagedType.LPUTF8Str)] string? tag);
    }
}
