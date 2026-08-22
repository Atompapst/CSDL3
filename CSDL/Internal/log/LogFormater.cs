// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using static CSDL.Log;
namespace CSDL.Internal.Logging {
    internal sealed class LogFormatter {
        private readonly Func<LogPriority, string> _prefixProvider;

        public LogFormatter(Func<LogPriority, string> prefixProvider) {
            _prefixProvider = prefixProvider ?? throw new ArgumentNullException(nameof(prefixProvider));
        }

        public string Format(in LogEntry entry) {
            string timestamp = entry.Timestamp.ToString();
            string prefix = _prefixProvider(entry.Priority);

            if (string.IsNullOrWhiteSpace(prefix)) {
                return $"[{timestamp}] [{entry.Category}] {entry.Message}";
            }

            return $"[{timestamp}] [{entry.Category}] {prefix} {entry.Message}";
        }
    }
}
