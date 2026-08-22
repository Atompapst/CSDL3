// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

namespace CSDL.Threads {
    public static class SpinLockExtension {
        /// <inheritdoc cref="CSDL.Internal.Docs.Atomic.LockSpinlock"/>
        public static void Lock(this ref SpinLock spinlock) {
            SDL.LockSpinlock(ref spinlock);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Atomic.TryLockSpinlock"/>
        public static bool TryLock(ref SpinLock spinlock) {
            return SDL.TryLockSpinlock(ref spinlock);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Atomic.UnlockSpinlock"/>
        public static void Unlock(ref SpinLock spinlock) {
            SDL.UnlockSpinlock(ref spinlock);
        }
    }
}
