// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Extensions;
namespace CSDL.Threads {
    /// <inheritdoc cref="CSDL.Internal.Docs.Mutex"/>
    public sealed class RWLock : NativeHandle<Opaque.SdlRwLock> {

        /// <inheritdoc cref="CSDL.Internal.Docs.Mutex.CreateRWLock"/>
        public RWLock() {
            Handle = SDL.CreateRWLock().ThrowIfInvalid();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mutex.LockRWLockForReading"/>
        public void LockForReading() {
            SDL.LockRWLockForReading(Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mutex.LockRWLockForWriting"/>
        public void LockForWriting() {
            SDL.LockRWLockForWriting(Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mutex.TryLockRWLockForReading"/>
        public bool TryLockForReading() {
            return SDL.TryLockRWLockForReading(Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mutex.TryLockRWLockForWriting"/>
        public bool TryLockForWriting() {
            return SDL.TryLockRWLockForWriting(Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mutex.UnlockRWLock"/>
        public void Unlock() {
            SDL.UnlockRWLock(Handle);
        }

        /// <summary>
        /// Acquire a read lock and return a scope guard that unlocks on Dispose.
        /// Usage: <c>using var _ = rwlock.AcquireReadScope();</c>
        /// </summary>
        public RWLockReadGuard AcquireReadScope() {
            LockForReading();
            return new RWLockReadGuard(this);
        }

        /// <summary>
        /// Acquire a write lock and return a scope guard that unlocks on Dispose.
        /// Usage: <c>using var _ = rwlock.AcquireWriteScope();</c>
        /// </summary>
        public RWLockWriteGuard AcquireWriteScope() {
            LockForWriting();
            return new RWLockWriteGuard(this);
        }

        /// <summary>
        /// Try to acquire a read lock without blocking. Returns true and a guard on success.
        /// </summary>
        public bool TryAcquireReadScope(out RWLockReadGuard guard) {
            if (TryLockForReading()) {
                guard = new RWLockReadGuard(this);
                return true;
            }
            guard = default;
            return false;
        }

        /// <summary>
        /// Try to acquire a write lock without blocking. Returns true and a guard on success.
        /// </summary>
        public bool TryAcquireWriteScope(out RWLockWriteGuard guard) {
            if (TryLockForWriting()) {
                guard = new RWLockWriteGuard(this);
                return true;
            }
            guard = default;
            return false;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mutex.DestroyRWLock"/>
        protected override void DisposeResource() {
            SDL.DestroyRWLock(Handle);
        }
    }
}
