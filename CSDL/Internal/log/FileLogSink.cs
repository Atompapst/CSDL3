// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using static CSDL.Log;
namespace CSDL.Internal.Logging {
    internal sealed class FileLogSink : ILogSink {
        private readonly LogFormatter _formatter;
        private readonly object _sync = new object();
        private readonly string _path;

        public FileLogSink(string path, LogFormatter formatter) {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("File path cannot be empty.", nameof(path));

            _path = path;
            _formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
        }

        public string Path {
            get {
                lock (_sync) {
                    return _path;
                }
            }
        }

        public void Write(in LogEntry entry) {
            string text = _formatter.Format(in entry);

            lock (_sync) {
                System.IO.File.AppendAllText(_path, text + Environment.NewLine);
            }
        }

        public void Flush() { }
    }
}
