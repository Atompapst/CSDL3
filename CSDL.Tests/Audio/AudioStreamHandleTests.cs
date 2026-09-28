// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CSDL;
using CSDL.Audio;
using CSDL3.Tests.TestSupport;
using Assert = Xunit.Assert;

namespace CSDL3.Tests.Audio {
    [Collection(SdlCollection.Name)]
    public sealed class AudioStreamHandleTests {
        private static readonly AudioSpec Spec = new AudioSpec {
            Format = AudioFormat.F32Le,
            Channels = 2,
            Freq = 48000,
        };

        [Fact]
        public void AStreamHandleIsAnEightByteValue() {
            Assert.Equal(sizeof(long), Unsafe.SizeOf<AudioStream>());
        }

        [Fact]
        public void TheDefaultHandleIsInert_WithoutLoadingSdl() {
            // No static constructor on the struct, so asking a zero handle anything must not pull the
            // audio subsystem up behind our back.
            Assert.True(default(AudioStream).IsDefault);
            Assert.False(default(AudioStream).IsValid);
            Assert.Equal(0, default(AudioStream).NativePointer);

            default(AudioStream).Dispose();
            default(AudioStream).Dispose();
        }

        [Fact]
        public void AZeroHandle_ThrowsWhenUsed() {
            Assert.Throws<ObjectDisposedException>(() => default(AudioStream).Available);
            Assert.Throws<ObjectDisposedException>(() => default(AudioStream).Flush());
        }

        [Fact]
        public void ADisposedStreamIsDistinguishableFromAZeroOne() {
            AudioStream stream = new AudioStream(Spec, Spec);
            Assert.True(stream.IsValid);
            Assert.False(stream.IsDefault);

            stream.Dispose();
            
            Assert.False(stream.IsValid);
            Assert.False(stream.IsDefault);
            Assert.Equal(0, stream.NativePointer);
        }

        [Fact]
        public void ACopyOfAStreamIsTheSameStream() {
            AudioStream stream = new AudioStream(Spec, Spec);
            try {
                List<AudioStream> list = [stream];
                Dictionary<AudioStream, string> map = new Dictionary<AudioStream, string> { [stream] = "voice" };
                Func<AudioStream> captured = () => stream;

                Assert.Equal(stream, list[0]);
                Assert.Equal("voice", map[stream]);
                Assert.True(captured() == stream);
                Assert.Equal(stream.NativePointer, list[0].NativePointer);
            } finally {
                stream.Dispose();
            }
        }

        [Fact]
        public void DisposingAStream_RetiresEveryCopyOfIt() {
            AudioStream stream = new AudioStream(Spec, Spec);
            AudioStream copy = stream;

            stream.Dispose();

            // Destruction is a property of the handle, not something done to one wrapper.
            Assert.False(copy.IsValid);
            Assert.Throws<ObjectDisposedException>(() => copy.Queued);

            // And the copy's Dispose must not free the stream a second time.
            copy.Dispose();
        }

        [Fact]
        public void TheFormatIsReadFromSdl_NotFromAConstructionTimeCache() {
            AudioSpec source = Spec;
            AudioSpec destination = new AudioSpec { Format = AudioFormat.S16Le, Channels = 1, Freq = 22050 };

            AudioStream stream = new AudioStream(source, destination);
            try {
                AudioStream copy = stream;

                Assert.Equal(destination.Freq, copy.DestinationSpec.Freq);
                Assert.Equal(destination.Channels, copy.DestinationSpec.Channels);

                AudioSpec changed = new AudioSpec { Format = AudioFormat.F32Le, Channels = 2, Freq = 44100 };
                Assert.True(stream.SetAudioStreamFormat(null, changed), CSDL.Error.GetError());
                
                Assert.Equal(changed.Freq, copy.DestinationSpec.Freq);
                Assert.Equal(source.Freq, copy.SourceSpec.Freq);
            } finally {
                stream.Dispose();
            }
        }

        [Fact]
        public void ACallbackRegistrationBelongsToTheStream_NotToTheWrapperThatSetIt() {
            AudioStream stream = new AudioStream(Spec, Spec);
            AudioStream copy = stream;
            int putCalls = 0;
            byte[] frame = new byte[8]; // one F32 stereo frame at Spec

            try {
                Assert.True(copy.SetPutCallback((_, _, _, _) => putCalls++), CSDL.Error.GetError());

                stream.PutData(frame);
                Assert.Equal(1, putCalls);
                
                Assert.True(stream.ClearPutCallback(), CSDL.Error.GetError());

                stream.PutData(frame);
                Assert.Equal(1, putCalls);
            } finally {
                stream.Dispose();
            }
        }

        [Fact]
        public void ReplacingACallback_LeavesExactlyOneCallbackRegistered() {
            AudioStream stream = new AudioStream(Spec, Spec);
            int firstPutCalls = 0, secondPutCalls = 0;
            int firstGetCalls = 0, secondGetCalls = 0;
            byte[] frame = new byte[8]; // one F32 stereo frame at Spec

            try {
                // Replacing a callback has to fully retire the previous registration rather than
                // stack on top of it - both callbacks firing would mean the registration was
                // duplicated instead of replaced.
                Assert.True(stream.SetPutCallback((_, _, _, _) => firstPutCalls++), CSDL.Error.GetError());
                Assert.True(stream.SetPutCallback((_, _, _, _) => secondPutCalls++), CSDL.Error.GetError());
                Assert.True(stream.SetGetCallback((_, _, _, _) => firstGetCalls++), CSDL.Error.GetError());
                Assert.True(stream.SetGetCallback((_, _, _, _) => secondGetCalls++), CSDL.Error.GetError());

                stream.PutData(frame);
                stream.GetData(frame);

                Assert.Equal(0, firstPutCalls);
                Assert.Equal(1, secondPutCalls);
                Assert.Equal(0, firstGetCalls);
                Assert.Equal(1, secondGetCalls);
            } finally {
                stream.Dispose();
            }
        }

        [Fact]
        public void ALockOnAStreamThatWasDisposedElsewhere_UnlocksNothing() {
            AudioStream stream = new AudioStream(Spec, Spec);
            AudioStreamLock guard = stream.AcquireLock();

            // The guard used to test its stream for null, which a value type always fails. Somebody
            // else disposing the stream out from under it has to leave the guard's own Dispose a
            // no-op rather than a throw.
            stream.Dispose();
            guard.Dispose();
        }

        [Fact]
        public void ADefaultLockUnlocksNothing() {
            default(AudioStreamLock).Dispose();
        }
    }
}
