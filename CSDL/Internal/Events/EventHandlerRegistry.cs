// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System.Collections.Generic;

namespace CSDL {
    /// <summary>
    /// Tracks which event handlers have been accessed and manages their lifecycle.
    /// </summary>
    internal static class EventHandlerRegistry {
        private static readonly HashSet<IEventCycle> _accessedHandlers = new HashSet<IEventCycle>();
        private static readonly object _lock = new object();

        /// <summary>
        /// Registers a handler as "accessed" so it will participate in BeginCycle.
        /// </summary>
        public static T MarkAccessed<T>(T handler) where T : IEventCycle {
            lock (_lock) {
                _accessedHandlers.Add(handler);
            }
            return handler;
        }

        /// <summary>
        /// Calls BeginCycle on all handlers that have been accessed at least once.
        /// </summary>
        public static void BeginCycleForAccessedHandlers() {
            lock (_lock) {
                foreach (IEventCycle handler in _accessedHandlers) {
                    handler.BeginCycle();
                }
            }
        }

        /// <summary>
        /// Clears the registry (useful for testing or reset scenarios).
        /// </summary>
        public static void Clear() {
            lock (_lock) {
                _accessedHandlers.Clear();
            }
        }
    }
}
