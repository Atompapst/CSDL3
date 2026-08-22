// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

namespace CSDL.Threads {
    public static class MemoryBarrier {
        /// <inheritdoc cref="CSDL.Internal.Docs.Atomic.MemoryBarrierReleaseFunction"/>
        public static void Release() {
            SDL.MemoryBarrierReleaseFunction();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Atomic.MemoryBarrierAcquireFunction"/>
        public static void Acquire() {
            SDL.MemoryBarrierAcquireFunction();
        }
    }
}
