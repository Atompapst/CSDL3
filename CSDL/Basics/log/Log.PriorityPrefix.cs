// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CSDL.Extensions;
namespace CSDL {

    public static partial class Log {
        /// <summary>
        /// Stores metadata for each SDL log priority.
        /// </summary>
        public static readonly IPriorityPrefixMap PriorityPrefix = Prefixes;

        private static readonly PriorityPrefixMap Prefixes = new PriorityPrefixMap();

        public interface IPriorityPrefixMap {
            /// <inheritdoc cref="PriorityPrefixMap.this"/>
            string? this[LogPriority priority] { get; set; }
        }

        internal class PriorityPrefixMap : IPriorityPrefixMap {
            private readonly Dictionary<LogPriority, PriorityPrefixItem> _dict = new Dictionary<LogPriority, PriorityPrefixItem>();
            
            public PriorityPrefixMap() {
                for (int i = (int)LogPriority.Trace; i < (int)LogPriority.Count; i++) {
                    LogPriority priority = (LogPriority)i;
                    _dict[priority] = new PriorityPrefixItem(priority, priority.ToString().ToUpper());
                }
            }
            
            /// <summary>
            /// Get or set the prefix for the given priority.
            /// </summary>
            public string? this[LogPriority priority] {
                get {
                    Ensure(priority);
                    return _dict[priority].Prefix;
                }
                set {
                    Ensure(priority);
                    _dict[priority].Prefix = value;
                }
            }

            public void Add(LogPriority priority, string prefix) {
                if (!_dict.ContainsKey(priority)) {
                    _dict[priority] = new PriorityPrefixItem(priority, prefix);
                }
            }

            public bool TryGet(LogPriority priority, [NotNullWhen(true)] out PriorityPrefixItem? props) {
                return _dict.TryGetValue(priority, out props);
            }

            private void Ensure(LogPriority priority) {
                if (!_dict.ContainsKey(priority)) {
                    _dict[priority] = new PriorityPrefixItem(priority, priority.ToString().ToUpperInvariant());
                }
            }
        }

        /// <summary>
        /// Stores metadata for a log priority.
        /// </summary>
        internal class PriorityPrefixItem {
            private readonly LogPriority _priority;
            // Stays null until SetLogPriorityPrefix successfully applies a non-blank prefix; the
            // constructor's call to it can leave this unset if the given prefix is null/whitespace.
            private string? _prefix;

            public string? Prefix {
                get => _prefix;
                set => SetLogPriorityPrefix(value);
            }

            internal PriorityPrefixItem(LogPriority priority, string prefix) {
                _priority = priority;
                SetLogPriorityPrefix(prefix);
            }

            private void SetLogPriorityPrefix(string? prefix) {
                if (string.IsNullOrWhiteSpace(prefix))
                    return;

                if (_prefix == prefix)
                    return;

                if (!SDL.SetLogPriorityPrefix(_priority, prefix).LogIfFalse(nameof(SetLogPriorityPrefix))) {
                    return;
                }

                _prefix = prefix;
            }

            public override string ToString() {
                return $"{nameof(Prefix)}: {Prefix}";
            }
        }
    }
}
