// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

namespace CSDL {
    /// <summary>
    /// Lightweight counter for stuff
    /// </summary>
    internal struct Counter {
        private int _count;

        public int Count => _count;

        public void Increment() {
            _count++;
        }

        public void Reset() {
            _count = 0;
        }

        public bool HasEvents => _count > 0;

        public static implicit operator int(Counter counter) {
            return counter._count;
        }
    }
}
