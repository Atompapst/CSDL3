using System.Collections.Generic;
using BenchmarkDotNet.Attributes;

namespace GenericBenchmark.Benchmarks {
    /// <summary>
    ///     Common shape for a benchmark that compares a managed CSDL_IMPL port against the native SDL
    ///     call it stands in for. Implement <see cref="Managed"/> and <see cref="Native"/> - each still
    ///     needs its own [Benchmark] attribute (BenchmarkDotNet's discovery only looks at attributes
    ///     declared directly on the method, not ones inherited from an abstract base) - then call
    ///     <see cref="ValidateManagedMatchesNative"/> from your own [GlobalSetup] (state each variant
    ///     needs - buffers, [Params]-driven fixtures, ... - has to be ready before that call).
    /// </summary>
    public abstract class ManagedVsNativeBenchmarks<T> {
        protected void ValidateManagedMatchesNative() {
            T managed = Managed();
            T native = Native();
            if (!EqualityComparer<T>.Default.Equals(managed, native)) {
                throw new InvalidOperationException($"{GetType().Name}: managed result ({managed}) differs from native result ({native}).");
            }
        }

        public abstract T Managed();

        public abstract T Native();
    }
}
