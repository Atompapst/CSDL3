// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

namespace CSDL.Threads {
    public partial struct AtomicU32 {
        public AtomicU32() : this(0) { }
        public AtomicU32(uint initialValue) {
            Value = initialValue;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Atomic.GetAtomicU32"/>
        /// <seealso cref="Value"/>
        public uint Get() {
            return SDL.GetAtomicU32(ref this);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Atomic.SetAtomicU32"/>
        /// <seealso cref="Value"/>
        public uint Set(uint v) {
            return SDL.SetAtomicU32(ref this, v);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Atomic.AddAtomicU32"/>
        public uint Add(int v) {
            return SDL.AddAtomicU32(ref this, v);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Atomic.CompareAndSwapAtomicU32"/>
        public bool CompareAndSwap(uint oldval, uint newval) {
            return SDL.CompareAndSwapAtomicU32(ref this, oldval, newval);
        }
    }
}
