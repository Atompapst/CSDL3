// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Audio;

namespace CSDL3.Tests.Audio {
    public class AudioSafetyTests {
        [Fact]
        public void AudioFormatInfo_ConstructsTheSdlDefinedFormat() {
            AudioFormatInfo format = new AudioFormatInfo(isFloat: true, isSigned: true, isBigEndian: false, size: 32);

            Assert.Equal(AudioFormat.F32Le, format.Format);
            Assert.True(format.IsFloat);
            Assert.True(format.IsSigned);
            Assert.True(format.IsLittleEndian);
        }

        [Fact]
        public void AudioClip_WithAnInvalidFrameSize_HasNoFrames() {
            using AudioClip clip = new AudioClip(default, new byte[4]);

            Assert.Equal(0, clip.FrameCount);
        }

        [Fact]
        public void AudioCallbacks_ExposeTheNativeBufferAsASpan() {
            AudioPostmixCallback callback = (object userData, in AudioSpec spec, System.Span<float> samples, int byteLength) => samples.Clear();

            Assert.NotNull(callback);
        }
    }
}
