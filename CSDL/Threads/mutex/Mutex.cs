// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Extensions;
namespace CSDL.Threads {
    /// <inheritdoc cref="CSDL.Internal.Docs.Mutex"/>
    public sealed class Mutex : NativeHandle<Opaque.SdlMutex> {
        /// <inheritdoc cref="CSDL.Internal.Docs.Mutex.CreateMutex"/>
        public Mutex() {
            Handle = SDL.CreateMutex().ThrowIfInvalid();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mutex.LockMutex"/>
        public void Lock() {
            SDL.LockMutex(Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mutex.TryLockMutex"/>
        public bool TryLock() {
            return SDL.TryLockMutex(Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mutex.UnlockMutex"/>
        public void Unlock() {
            SDL.UnlockMutex(Handle);
        }

        /// <summary>
        /// Acquire the mutex and return a scope guard that unlocks on Dispose.
        /// Usage: <c>using var _ = mutex.AcquireScope();</c>
        /// </summary>
        public Guard AcquireScope() {
            Lock();
            return new Guard(this);
        }

        /// <summary>
        /// Try to acquire the mutex without blocking. Returns true and a valid guard on success.
        /// </summary>
        public bool TryAcquireScope(out Guard guard) {
            if (SDL.TryLockMutex(Handle)) {
                guard = new Guard(this);
                return true;
            }
            guard = default;
            return false;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mutex.DestroyMutex"/>
        protected override void DisposeResource() {
            SDL.DestroyMutex(Handle);
        }
    }
}
