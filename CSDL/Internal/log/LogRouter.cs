// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Extensions;
using static CSDL.Log;
namespace CSDL.Internal.Logging {
    internal sealed class LogRouter {
        private readonly SDL_LogOutputFunctionNative _defaultOutput;
        private readonly SinkPipeline _pipeline;
        private volatile LogOutputMode _mode;

        public LogRouter(SinkPipeline pipeline) {
            _pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
            _defaultOutput = SDL.GetDefaultLogOutputFunction();
            _mode = LogOutputMode.Default;
        }

        public LogOutputMode Mode {
            get => _mode;
            set => _mode = value;
        }

        public void Handle(LogCategory category, LogPriority priority, NativePtr<byte> rawMessage) {
            if (_pipeline.HasSinks) {
                LogEntry entry = new LogEntry(
                    DateTime.Now,
                    category,
                    priority,
                    rawMessage.ToUtf8String() ?? string.Empty);

                _pipeline.Dispatch(entry);
            }

            if (_mode == LogOutputMode.Default) {
                _defaultOutput(nint.Zero, (int)category, priority, rawMessage.Ptr);
            }
        }
    }
}
