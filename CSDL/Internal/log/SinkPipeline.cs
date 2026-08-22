// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Collections.Generic;
using static CSDL.Log;

namespace CSDL.Internal.Logging {
    internal sealed class SinkPipeline {
        private readonly List<ILogSink> _sinks = new List<ILogSink>();
        private readonly object _sync = new object();

        /// <summary>
        /// Indicates whether any sink is currently registered.
        /// </summary>
        public bool HasSinks {
            get {
                lock (_sync) {
                    return _sinks.Count > 0;
                }
            }
        }

        public void AddSink(ILogSink sink) {
            if (sink == null) throw new ArgumentNullException(nameof(sink));

            lock (_sync) {
                _sinks.Add(sink);
            }
        }

        /// <summary>
        /// Removes the given sink instance, if it is registered.
        /// </summary>
        /// <returns><see langword="true"/> if the sink was found and removed.</returns>
        public bool RemoveSink(ILogSink sink) {
            if (sink == null) throw new ArgumentNullException(nameof(sink));

            lock (_sync) {
                return _sinks.Remove(sink);
            }
        }

        public void RemoveSink<T>() where T : class, ILogSink {
            lock (_sync) {
                for (int i = _sinks.Count - 1; i >= 0; i--) {
                    if (_sinks[i] is T) {
                        _sinks.RemoveAt(i);
                    }
                }
            }
        }

        public bool Contains<T>() where T : class, ILogSink {
            lock (_sync) {
                return _sinks.Exists(s => s is T);
            }
        }

        public void Dispatch(in LogEntry entry) {
            ILogSink[] snapshot;
            lock (_sync) {
                if (_sinks.Count == 0) return;
                snapshot = _sinks.ToArray();
            }

            foreach (ILogSink sink in snapshot) {
                try {
                    sink.Write(in entry);
                } catch { /* ignored */}
            }
        }

        public void Flush() {
            ILogSink[] snapshot;
            lock (_sync) {
                snapshot = _sinks.ToArray();
            }

            foreach (ILogSink sink in snapshot) {
                sink.Flush();
            }
        }
    }
}
