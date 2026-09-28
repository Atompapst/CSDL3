// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL;
using CSDL.Audio;
using CSDL3.Tests.TestSupport;
using Assert = Xunit.Assert;

namespace CSDL3.Tests.Audio {
    [Collection(SdlCollection.Name)]
    public class PlaybackDeviceNativeTests {
        public PlaybackDeviceNativeTests(SdlFixture fixture) {
            // The dummy driver accepts playback without requiring audio hardware.
            Hints.AudioDriver.Set("dummy");
        }

        [Fact]
        public void Write_ClipsWithDifferentFormats_AcceptsCompleteSourceFrames() {
            using PlaybackDevice playback = PlaybackDevice.OpenDefault(new AudioSpec(AudioFormats.F32, 48000, 2));
            using AudioClip mono16 = new AudioClip(new AudioSpec(AudioFormats.S16, 22050, 1), new byte[202]);
            using AudioClip mono8 = new AudioClip(new AudioSpec(AudioFormat.U8, 32000, 1), new byte[101]);
            
            Error.ClearError();
            playback.Write(mono16);
            Assert.Equal(string.Empty, Error.GetError());

            playback.Write(mono8);
            Assert.Equal(string.Empty, Error.GetError());
        }

        [Fact]
        public void Write_AClip_TellsTheStreamTheClipsFormat_NotOnlyTheDevices() {
            // The quiet half of the same bug. 64 bytes happen to be a whole number of the device's
            // eight-byte frames, so handing them over without announcing the clip's format would be
            // silently accepted - and then treated as float samples instead of the eight-bit ones
            // they are. The clip's channels and rate already match the device, so its declared
            // format is the only thing that can grow each frame from two bytes to eight; if that
            // growth doesn't happen, the clip's format was never told to the stream.
            using PlaybackDevice playback = PlaybackDevice.OpenDefault(new AudioSpec(AudioFormats.F32, 48000, 2));
            playback.Pause(); // nothing may drain the queue between writing and reading it back

            AudioSpec clipSpec = new AudioSpec(AudioFormat.U8, 48000, 2);
            using AudioClip clip = new AudioClip(clipSpec, new byte[64]);

            Error.ClearError();
            playback.Write(clip);
            Assert.Equal(string.Empty, Error.GetError());

            // 64 bytes of U8 stereo is 32 frames; converted to F32 stereo (four bytes per channel,
            // no resampling since the rate already matches) that is 256 convertible bytes - not the
            // 64 bytes that would result from treating the data as already being in the device's
            // format. QueuedBytes counts raw input either way, so the conversion only shows up in
            // what is available to read back out.
            Assert.Equal(256, playback.AvailableBytes);

            // The device end is SDL's to keep, and writing a clip must not have moved it.
            Assert.Equal(AudioFormats.F32, playback.Spec.Format);
            Assert.Equal(2, playback.Spec.Channels);
        }
    }
}
