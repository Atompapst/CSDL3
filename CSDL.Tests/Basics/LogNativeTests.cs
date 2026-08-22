// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL3.Tests.TestSupport;
using Sdl = CSDL;

namespace CSDL3.Tests.Basics {
    [Collection(SdlCollection.Name)]
    public sealed class LogNativeTests {
        [Fact]
        public void Info_TreatsPercentCharactersAsLiteralText() {
            CapturingSink sink = new CapturingSink();
            Sdl.Log.AddSink(sink);
            try {
                Sdl.Log.Info("Loading 100% complete: %s");

                Assert.Equal("Loading 100% complete: %s", sink.Message);
            }
            finally {
                Sdl.Log.RemoveSink(sink);
            }
        }

        [Fact]
        public void ThrowingSink_DoesNotPreventOtherSinksFromReceivingTheEntry() {
            ThrowingSink throwingSink = new ThrowingSink();
            CapturingSink receivingSink = new CapturingSink();
            Sdl.Log.AddSink(throwingSink);
            Sdl.Log.AddSink(receivingSink);
            try {
                Sdl.Log.Warn("A log sink may fail.");

                Assert.Equal("A log sink may fail.", receivingSink.Message);
            }
            finally {
                Sdl.Log.RemoveSink(receivingSink);
                Sdl.Log.RemoveSink(throwingSink);
            }
        }

        private sealed class CapturingSink : Sdl.Log.ILogSink {
            public string Message { get; private set; } = string.Empty;

            public void Write(in Sdl.Log.LogEntry entry) {
                Message = entry.Message;
            }

            public void Flush() { }
        }

        private sealed class ThrowingSink : Sdl.Log.ILogSink {
            public void Write(in Sdl.Log.LogEntry entry) {
                throw new System.InvalidOperationException("Expected test exception.");
            }

            public void Flush() { }
        }
    }
}
