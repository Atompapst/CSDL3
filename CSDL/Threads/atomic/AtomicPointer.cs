// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
namespace CSDL.Threads {
    public struct AtomicPointer {
        private IntPtr _ptr;

        public AtomicPointer() : this(IntPtr.Zero) { }
        public AtomicPointer(IntPtr ptr) {
            _ptr = ptr;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Atomic.SetAtomicPointer"/>
        public IntPtr Value {
            get => Get();
            set => Set(value);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Atomic.GetAtomicPointer"/>
        /// <seealso cref="Value"/>
        public IntPtr Get() {
            return SDL.GetAtomicPointer(ref _ptr);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Atomic.SetAtomicPointer"/>
        public IntPtr Set(IntPtr v) {
            return SDL.SetAtomicPointer(ref _ptr, v);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Atomic.CompareAndSwapAtomicPointer"/>
        public bool CompareAndSwap(IntPtr oldval, IntPtr newval) {
            return SDL.CompareAndSwapAtomicPointer(ref _ptr, oldval, newval);
        }
    }
}
