// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;

namespace CSDL.Threads {
    /// <summary>
    /// Scope guard that unlocks a read lock on Dispose.
    /// </summary>
    public readonly struct RWLockReadGuard : IDisposable {
        private readonly RWLock _rwlock;

        internal RWLockReadGuard(RWLock rwlock) {
            _rwlock = rwlock;
        }

        public void Dispose() {
            if (_rwlock != null && !_rwlock.Handle.IsNull) {
                _rwlock.Unlock();
            }
        }
    }
}
