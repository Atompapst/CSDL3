// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

namespace CSDL {
    /// <summary>
    /// Random instance using SDL's random number generation Functions.
    /// </summary>
    /// <seealso cref="CSDL.Internal.Docs.Stdinc.srand">srand</seealso>
    /// <seealso cref="CSDL.Internal.Docs.Stdinc.rand">rand</seealso>
    /// <seealso cref="CSDL.Internal.Docs.Stdinc.rand_r">rand_r</seealso>
    /// <seealso cref="CSDL.Internal.Docs.Stdinc.randf">randf</seealso>
    /// <seealso cref="CSDL.Internal.Docs.Stdinc.randf_r">randf_r</seealso>
    /// <seealso cref="CSDL.Internal.Docs.Stdinc.rand_bits">rand_bits</seealso>
    /// <seealso cref="CSDL.Internal.Docs.Stdinc.rand_bits_r">rand_bits_r</seealso>
    public class Rand {
        private ulong _state;
        private readonly bool _useGlobalState;

        /// <summary>
        /// A shared <see cref="Rand"/> backed by SDL's global random state.
        /// </summary>
        /// <remarks>
        /// Unlike <see cref="System.Random.Shared"/>, this is <b>not</b> safe to use from multiple
        /// threads. SDL's global state (<c>SDL_rand</c>/<c>SDL_randf</c>/<c>SDL_rand_bits</c>) is one
        /// unsynchronized process-wide variable with no lock, so calling into it concurrently races.
        /// Restrict use of <see cref="Global"/> to a single thread, or create your own
        /// <see cref="Rand"/> per thread instead - every instance you construct yourself has its own
        /// independent state and cannot alias this one.
        /// </remarks>
        public static Rand Global { get; } = new Rand(null, true);

        /// <summary>
        /// Creates a new Random instance with its own independent, per-instance state.
        /// </summary>
        /// <param name="seed">Optional seed value. If null, uses SDL's default seeding.</param>
        /// <remarks>
        /// The instance's state lives only in this object, so it never aliases <see cref="Global"/>
        /// or any other instance - safe to keep one per thread. A single instance is still not safe
        /// to call concurrently from multiple threads at once.
        /// </remarks>
        public Rand(ulong? seed = null) : this(seed, false) { }

        private Rand(ulong? seed, bool useGlobalState) {
            _useGlobalState = useGlobalState;

            if (useGlobalState) {
                // Use SDL's global random state
                if (seed.HasValue) {
                    SDL.srand(seed.Value);
                } else {
                    // Let SDL use its default seeding (GetPerformanceCounter)
                    SDL.srand(0);
                }
            } else {
                // Use per-instance state for thread safety
                _state = seed ?? SDL.GetPerformanceCounter();
            }
        }

        /// <summary>
        /// Gets or sets the seed. Only affects global state mode.
        /// </summary>
        public ulong Seed {
            get => _useGlobalState ? 0 : _state;
            set {
                if (_useGlobalState) {
                    SDL.srand(value);
                } else {
                    _state = value;
                }
            }
        }



        /// <summary>
        /// Generates a random integer in the range [min, max].
        /// </summary>
        public int Next(int min, int max) {
            if (max <= min) return min;
            int range = max - min;
            return NextInt(range) + min;
        }

        /// <summary>
        /// Generates a random integer in the range [0, max].
        /// </summary>
        public int Next(int max) {
            return NextInt(max);
        }

        private int NextInt(int max) {
            if (max <= 0) return 0;
            if (_useGlobalState) {
                return SDL.rand(max);
            }
            return RandR(max);
        }

        /// <summary>
        /// Calls SDL_rand_r with the instance state. <c>_state</c> is a heap field, so its address
        /// cannot be handed to native code directly - a GC compaction mid-call would leave SDL
        /// writing into freed memory. Copying to a local (stack, non-movable for the call) and
        /// writing the result back keeps this sound.
        /// </summary>
        private int RandR(int max) {
            unsafe {
                fixed (ulong* s = &_state) {
                    return SDL.rand_r(s, max);
                }
            }
        }

        /// <inheritdoc cref="RandR"/>
        private uint RandBitsR() {
            unsafe {
                fixed (ulong* s = &_state) {
                    return SDL.rand_bits_r(s);
                }
            }
        }

        /// <inheritdoc cref="RandR"/>
        private float RandFR() {
            unsafe {
                fixed (ulong* s = &_state) {
                    return SDL.randf_r(s);
                }
            }
        }

        /// <summary>
        /// Generates a random integer in the full range of int [0 to <see cref="int.MaxValue"/>].
        /// </summary>
        public int NextInt() {
            uint bits;
            if (_useGlobalState) {
                bits = SDL.rand_bits();
            } else {
                bits = RandBitsR();
            }
            // Convert to positive int range
            return (int)(bits % int.MaxValue);
        }


        /// <summary>
        /// Generates a random floating-point number in the range [0.0, 1.0].
        /// </summary>
        public float NextFloat() {
            if (_useGlobalState) {
                return SDL.randf();
            }
            return RandFR();
        }

        /// <summary>
        /// Generates a random double in the range [0.0, 1.0].
        /// </summary>
        public double NextDouble() {
            return NextFloat();
        }

        /// <summary>
        /// Generates a random boolean value.
        /// </summary>
        public bool NextBool() {
            uint bits;
            if (_useGlobalState) {
                bits = SDL.rand_bits();
            } else {
                bits = RandBitsR();
            }
            return (bits & 1) == 1;
        }

        /// <summary>
        /// Fills the provided byte array with random bytes.
        /// </summary>
        public void NextBytes(byte[] buffer) {
            if (buffer == null) return;

            for (int i = 0; i < buffer.Length; i++) {
                uint bits;
                if (_useGlobalState) {
                    bits = SDL.rand_bits();
                } else {
                    bits = RandBitsR();
                }
                buffer[i] = (byte)(bits & 0xFF);
            }
        }

        /// <summary>
        /// Generates random 32-bit unsigned integer (full range).
        /// </summary>
        public uint NextUInt32() {
            if (_useGlobalState) {
                return SDL.rand_bits();
            }
            return RandBitsR();
        }

        /// <summary>
        /// Generates a random float in the specified range [min, max].
        /// </summary>
        public float Next(float min, float max) {
            if (max <= min) return min;
            return NextFloat() * (max - min) + min;
        }

        /// <summary>
        /// Generates a random float in the range [0.0, max].
        /// </summary>
        public float Next(float max) {
            if (max <= 0) return 0;
            return NextFloat() * (max - 0) + 0;
        }

        /// <summary>
        /// Generates a random double in the specified range [min, max].
        /// </summary>
        public double Next(double min, double max) {
            if (max <= min) return min;
            return NextDouble() * (max - min) + min;
        }

        /// <summary>
        /// Generates a random double in the range [0.0, max].
        /// </summary>
        public double Next(double max) {
            if (max <= 0) return 0;
            return NextDouble() * (max - 0) + 0;
        }

        /// <summary>
        /// Simulates rolling a die with the specified number of sides.
        /// Returns a value from 1 to sides (inclusive).
        /// </summary>
        public int RollDie(int sides) {
            if (sides <= 0) return 1;
            return NextInt(sides) + 1;
        }
    }
}
