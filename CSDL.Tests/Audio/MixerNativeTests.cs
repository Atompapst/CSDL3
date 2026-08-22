// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Audio;
using CSDL3.Tests.TestSupport;
using MixerApi = CSDL.Mixer.Mixer;

namespace CSDL3.Tests.Audio {
    /// <summary>
    /// Proves the configured SDL3_mixer runtime works. Everything here runs on a
    /// device-less mixer - <c>MIX_CreateMixer</c> rather than <c>MIX_CreateMixerDevice</c> - so the
    /// tests mix real PCM without needing an audio device, which a CI runner does not have.
    /// </summary>
    [Collection(SdlCollection.Name)]
    public class MixerNativeTests {
        /// <summary>48 kHz stereo 16-bit: the format the mixer works in for these tests.</summary>
        private static AudioSpec TestSpec => new AudioSpec(AudioFormats.S16, 48000, 2);

        private const int SampleRate = 48000;
        private const int Channels = 2;
        private const int BytesPerSample = 2;

        [Fact]
        public void Version_FromNativeLibrary_ReportsAnSdlMixer3Runtime() {
            SdlVersionNumber version = new SdlVersionNumber(MixerApi.Version);

            Assert.Equal(3, version.Major);
            Assert.Equal((int)MixerApi.MajorVersion, version.Major);
        }

        [Fact]
        public void VersionAtLeast_AgainstTheHeaderVersionTheBindingsUse_IsSatisfied() {
            Assert.True(
                MixerApi.VersionAtLeast(MixerApi.MajorVersion, MixerApi.MinorVersion, MixerApi.MicroVersion),
                $"native SDL_mixer is older than the {MixerApi.MajorVersion}.{MixerApi.MinorVersion}.{MixerApi.MicroVersion} headers the bindings were generated from");
        }

        [Fact]
        public void CreateMixer_WithoutAnAudioDevice_ProducesAUsableMixer() {
            // The constructor throws on a null handle, so reaching the assertions is itself the
            // proof that MIX_Init and MIX_CreateMixer both succeeded.
            using (MixerApi mixer = new MixerApi(TestSpec)) {
                Assert.True(mixer.GetFormat(out AudioSpec actual), CSDL.Error.GetError());
                Assert.Equal(TestSpec, actual);
                Assert.NotNull(mixer.Properties);
            }
        }

        [Fact]
        public void Generate_WithASineWavePlaying_WritesNonSilentPcmIntoTheBuffer() {
            // The end-to-end proof for SDL_mixer: it decodes, mixes and hands back real samples.
            const int milliseconds = 100;
            byte[] buffer = new byte[BytesToHold(milliseconds)];

            using (MixerApi mixer = new MixerApi(TestSpec))
            using (CSDL.Mixer.Audio tone = CSDL.Mixer.Audio.CreateSineWave(mixer, 440, 0.5f, milliseconds))
            using (CSDL.Mixer.Track track = mixer.CreateTrack()) {
                Assert.True(track.SetAudio(tone), CSDL.Error.GetError());
                Assert.True(track.Play(), CSDL.Error.GetError());

                int written = mixer.Generate(buffer);

                Assert.InRange(written, 0, buffer.Length);
                Assert.Contains(true, System.Linq.Enumerable.Select(buffer, b => b != 0));
                Assert.True(PeakAmplitude(buffer) > 0.1f, "the mixed output is effectively silent");
            }
        }

        [Fact]
        public void Generate_WithNothingPlaying_WritesSilenceAndReportsNothingMixed() {
            // The counterpart to the test above: a non-silent result there only means something if
            // an idle mixer is genuinely quiet. An idle mixer overwrites the buffer with silence
            // but reports zero bytes *mixed*, so the pre-filled marker must be gone and the return
            // value must still be 0 - -1 would mean the native call failed outright.
            byte[] buffer = new byte[BytesToHold(20)];
            System.Array.Fill(buffer, (byte)0xAB);

            using (MixerApi mixer = new MixerApi(TestSpec)) {
                int written = mixer.Generate(buffer);

                Assert.Equal(0, written);
                Assert.All(buffer, sample => Assert.Equal(0, sample));
            }
        }

        [Fact]
        public void SetGain_ThenReadItBack_RoundTripsThroughTheNativeMixerState() {
            using (MixerApi mixer = new MixerApi(TestSpec)) {
                Assert.Equal(1f, mixer.Gain, 1e-4f);

                mixer.Gain = 0.25f;

                Assert.Equal(0.25f, mixer.Gain, 1e-4f);
            }
        }

        [Fact]
        public void CreateSineWave_ThenReadItsDuration_ReportsTheRequestedLengthInFrames() {
            const int milliseconds = 250;

            using (MixerApi mixer = new MixerApi(TestSpec))
            using (CSDL.Mixer.Audio tone = CSDL.Mixer.Audio.CreateSineWave(mixer, 440, 0.5f, milliseconds)) {
                long expectedFrames = MixerApi.MSToFrames(SampleRate, milliseconds);

                Assert.Equal(expectedFrames, tone.DurationFrames);
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(500)]
        [InlineData(60_000)]
        public void MSToFrames_ThenFramesToMS_RoundTripsThroughTheNativeConversions(long milliseconds) {
            long frames = MixerApi.MSToFrames(SampleRate, milliseconds);

            Assert.Equal(milliseconds, MixerApi.FramesToMS(SampleRate, frames));
        }

        [Fact]
        public void MSToFrames_AtTheTestSampleRate_MatchesTheArithmeticSdlMixerDocuments() {
            Assert.Equal(SampleRate, MixerApi.MSToFrames(SampleRate, 1000));
            Assert.Equal(SampleRate / 2, MixerApi.MSToFrames(SampleRate, 500));
        }

        [Fact]
        public void CreateTrack_ThenPlayAndStop_IsReflectedInTheNativeTrackState() {
            using (MixerApi mixer = new MixerApi(TestSpec))
            using (CSDL.Mixer.Audio tone = CSDL.Mixer.Audio.CreateSineWave(mixer, 220, 0.4f, 500))
            using (CSDL.Mixer.Track track = mixer.CreateTrack()) {
                Assert.False(track.IsPlaying);

                Assert.True(track.SetAudio(tone), CSDL.Error.GetError());
                Assert.True(track.Play(), CSDL.Error.GetError());
                Assert.True(track.IsPlaying);

                Assert.True(track.Stop(), CSDL.Error.GetError());
                Assert.False(track.IsPlaying);
            }
        }

        [Fact]
        public void CreateGroup_ThenAssignATrackToIt_Succeeds() {
            using (MixerApi mixer = new MixerApi(TestSpec))
            using (CSDL.Mixer.Group group = mixer.CreateGroup())
            using (CSDL.Mixer.Track track = mixer.CreateTrack()) {
                Assert.True(track.SetGroup(group), CSDL.Error.GetError());

                // Detaching has to work too, or the group would outlive its members.
                Assert.True(track.SetGroup(null), CSDL.Error.GetError());
            }
        }

        private static int BytesToHold(int milliseconds) {
            return (int)MixerApi.MSToFrames(SampleRate, milliseconds) * Channels * BytesPerSample;
        }

        /// <summary>
        /// The largest absolute sample in an interleaved signed-16-bit little-endian buffer,
        /// normalised to 0..1.
        /// </summary>
        private static float PeakAmplitude(byte[] pcm) {
            short peak = 0;
            for (int i = 0; i + 1 < pcm.Length; i += 2) {
                short sample = (short)(pcm[i] | (pcm[i + 1] << 8));
                int magnitude = sample == short.MinValue ? short.MaxValue : System.Math.Abs(sample);
                if (magnitude > peak) {
                    peak = (short)magnitude;
                }
            }
            return peak / (float)short.MaxValue;
        }
    }
}
