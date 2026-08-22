// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
namespace CSDL.Threads {
    /// <summary>
    /// Scope guard that unlocks the mutex on Dispose.
    /// </summary>
    public readonly struct Guard : IDisposable {
        private readonly Mutex _mutex;

        internal Guard(Mutex mutex) {
            _mutex = mutex;
        }

        public void Dispose() {
            if (_mutex != null && !_mutex.Handle.IsNull) {
                _mutex.Unlock();
            }
        }
    }
}
