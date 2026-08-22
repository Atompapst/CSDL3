// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

namespace CSDL.Threads {
    public partial struct AtomicInt {
        public AtomicInt() : this(0) { }
        public AtomicInt(int initialValue) {
            Value = initialValue;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Atomic.GetAtomicInt"/>
        public int Get() {
            return SDL.GetAtomicInt(ref this);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Atomic.SetAtomicInt"/>
        public int Set(int v) {
            return SDL.SetAtomicInt(ref this, v);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Atomic.AddAtomicInt"/>
        public int Add(int v) {
            return SDL.AddAtomicInt(ref this, v);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Atomic.CompareAndSwapAtomicInt"/>
        public bool CompareAndSwap(int oldVal, int newVal) {
            return SDL.CompareAndSwapAtomicInt(ref this, oldVal, newVal);
        }

        /// <inheritdoc cref="CSDL.Threads.Macros.AtomicIncRef"/>
        public int IncRef => Macros.AtomicIncRef(ref this);

        /// <inheritdoc cref="CSDL.Threads.Macros.AtomicDecRef"/>
        public bool DecRef => Macros.AtomicDecRef(ref this);
    }
}
