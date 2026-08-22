// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;

namespace CSDL.Threads {
    /// <summary>
    /// Scope guard that unlocks a write lock on Dispose.
    /// </summary>
    public readonly struct RWLockWriteGuard : IDisposable {
        private readonly RWLock _rwlock;

        internal RWLockWriteGuard(RWLock rwlock) {
            _rwlock = rwlock;
        }

        public void Dispose() {
            if (_rwlock != null && !_rwlock.Handle.IsNull) {
                _rwlock.Unlock();
            }
        }
    }
}
