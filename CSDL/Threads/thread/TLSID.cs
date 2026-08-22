// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Extensions;

namespace CSDL.Threads {
    /// <summary>
    /// Extension methods for <see cref="TLSID"/> - a thread-local-storage slot. Declare one
    /// (typically as a <see langword="static"/> field, left at its default value) and use it from
    /// any thread; SDL lazily assigns the slot the first time it's used via <see cref="Get"/> or
    /// <see cref="Set"/>, and each thread thereafter gets its own independent value for it.
    /// </summary>
    public static class TLSIDExtension {
        /// <inheritdoc cref="CSDL.Internal.Docs.Thread.GetTLS"/>
        public static IntPtr Get(this ref TLSID id) {
            AtomicInt slot = id.Value;
            IntPtr value = SDL.GetTLS(ref slot);
            id = slot;
            return value;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Thread.SetTLS"/>
        public static bool Set(this ref TLSID id, IntPtr value, TLSDestructorCallback? destructor = null) {
            AtomicInt slot = id.Value;

            SDL_TLSDestructorCallbackNative native = null!;
            if (destructor != null) {
                native = TLSDestructorCallbackWrapper.Create(destructor);
                CallbackRegistry.Register($"TLS:{Guid.NewGuid()}", destructor, native);
            }

            bool ok = SDL.SetTLS(ref slot, value, native).LogIfFalse();
            id = slot;
            return ok;
        }
    }
}
