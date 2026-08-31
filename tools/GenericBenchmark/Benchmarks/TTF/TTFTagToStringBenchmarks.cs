extern alias RealCSDL;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using TTF = RealCSDL::CSDL.TTF.TTF;

namespace GenericBenchmark.Benchmarks {
    /// <summary>
    ///     Managed CSDL_IMPL port (<see cref="TTF.TagToString"/>, CSDL/TTF/TTF.CSDL_IMPL.cs) vs the
    ///     native TTF_TagToString it stands in for, using the same stackalloc-buffer-plus-P/Invoke
    ///     shape the old wrapper used, so this measures exactly the overhead the port removed.
    /// </summary>
    [MediumRunJob]
    [MemoryDiagnoser]
    public unsafe partial class TTFTagToStringBenchmarks : ManagedVsNativeBenchmarks<string> {
        private static readonly uint[] Tags = {
            TTF.StringToTag("kern"), TTF.StringToTag("liga"), TTF.StringToTag("ss01"),
            TTF.StringToTag("DFLT"), TTF.StringToTag("latn"), TTF.StringToTag("aalt"),
            TTF.StringToTag("en"), TTF.StringToTag("de-DE"),
        };

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
            foreach (uint tag in Tags) {
                string managed = TTF.TagToString(tag);
                string native = NativeTagToString(tag);
                if (managed != native) {
                    throw new InvalidOperationException(
                        $"TagToString(0x{tag:X8}): managed result ('{managed}') differs from native result ('{native}').");
                }
            }
        }

        [Benchmark(Baseline = true)]
        public override string Managed() {
            return TTF.TagToString(Tags[_counter++ & 7]);
        }

        [Benchmark]
        public override string Native() {
            return NativeTagToString(Tags[_counter++ & 7]);
        }

        private static string NativeTagToString(uint tag) {
            Span<byte> buffer = stackalloc byte[8];
            fixed (byte* p = buffer) {
                TagToString(tag, p, (nuint)buffer.Length);
                return Marshal.PtrToStringUTF8((IntPtr)p) ?? string.Empty;
            }
        }

        [LibraryImport("SDL3_ttf", EntryPoint = "TTF_TagToString")]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        private static partial void TagToString(uint tag, byte* buf, nuint size);
    }
}
