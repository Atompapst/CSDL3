// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;

namespace CSDL {
    public static partial class Log {
        /// <summary>
        /// A single formatted log message dispatched to registered <see cref="ILogSink"/> instances.
        /// </summary>
        public readonly struct LogEntry {
            public LogEntry(DateTime timestamp, LogCategory category, LogPriority priority, string message) {
                Timestamp = timestamp;
                Category = category;
                Priority = priority;
                Message = message ?? string.Empty;
            }

            public DateTime Timestamp { get; }
            public LogCategory Category { get; }
            public LogPriority Priority { get; }
            public string Message { get; }
        }

        /// <summary>
        /// Receives formatted log entries dispatched by <see cref="Log"/>.
        /// </summary>
        /// <remarks>
        /// Register a custom sink with <see cref="AddSink"/> to route log output to additional
        /// destinations alongside the built-in console/file sinks.
        /// </remarks>
        public interface ILogSink {
            /// <summary>
            /// Writes the given log entry to the sink's destination.
            /// </summary>
            void Write(in LogEntry entry);

            /// <summary>
            /// Flushes any buffered output.
            /// </summary>
            void Flush();
        }
    }
}
