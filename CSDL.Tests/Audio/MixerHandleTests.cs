// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Runtime.CompilerServices;
using CSDL.Mixer;
using Assert = Xunit.Assert;
using MixerApi = CSDL.Mixer.Mixer;

namespace CSDL3.Tests.Audio {
    /// <summary>
    ///     The mixer handles as value types.
    /// </summary>
    /// <remarks>
    ///     Deliberately outside <c>SdlCollection</c> and free of any native call: SDL_mixer is not
    ///     present on every machine that runs these tests, and none of this needs it. That is itself
    ///     the point worth pinning - the old wrappers loaded SDL_mixer from a static constructor, so
    ///     merely asking a default handle whether it was valid pulled the library in. Every test here
    ///     would have failed with a DllNotFoundException before this step.
    /// </remarks>
    public sealed class MixerHandleTests {
        [Fact]
        public void EveryMixerHandleIsAnEightByteValue() {
            Assert.Equal(sizeof(long), Unsafe.SizeOf<MixerApi>());
            Assert.Equal(sizeof(long), Unsafe.SizeOf<Group>());
            Assert.Equal(sizeof(long), Unsafe.SizeOf<CSDL.Mixer.Audio>());
            Assert.Equal(sizeof(long), Unsafe.SizeOf<AudioDecoder>());
            Assert.Equal(sizeof(long), Unsafe.SizeOf<Track>());
        }

        [Fact]
        public void TheDefaultHandlesAreInert_WithoutLoadingSdlMixer() {
            Assert.True(default(MixerApi).IsDefault);
            Assert.True(default(Group).IsDefault);
            Assert.True(default(CSDL.Mixer.Audio).IsDefault);
            Assert.True(default(AudioDecoder).IsDefault);
            Assert.True(default(Track).IsDefault);

            Assert.False(default(MixerApi).IsValid);
            Assert.False(default(Group).IsValid);
            Assert.False(default(CSDL.Mixer.Audio).IsValid);
            Assert.False(default(AudioDecoder).IsValid);
            Assert.False(default(Track).IsValid);

            Assert.Equal(0, default(MixerApi).NativePointer);
            Assert.Equal(0, default(Group).NativePointer);
        }

        [Fact]
        public void DisposingAZeroHandleNeverReachesSdl() {
            default(MixerApi).Dispose();
            default(MixerApi).Dispose();
            default(Group).Dispose();
            default(CSDL.Mixer.Audio).Dispose();
            default(AudioDecoder).Dispose();
            default(Track).Dispose();
        }

        [Fact]
        public void AZeroHandle_ThrowsWhenUsed() {
            Assert.Throws<ObjectDisposedException>(() => default(MixerApi).Gain);
            Assert.Throws<ObjectDisposedException>(() => default(Group).ClearPostMixCallback());
            Assert.Throws<ObjectDisposedException>(() => default(CSDL.Mixer.Audio).DurationFrames);
            Assert.Throws<ObjectDisposedException>(() => default(Track).IsPlaying);
            Assert.Throws<ObjectDisposedException>(() => default(Track).ClearInput());
        }

        [Fact]
        public void HandlesCompareByIdentity() {
            Assert.Equal(default(MixerApi), default(MixerApi));
            Assert.True(default(Group) == default(Group));
            Assert.False(default(Group) != default(Group));
            Assert.Equal(default(AudioDecoder).GetHashCode(), default(AudioDecoder).GetHashCode());
            Assert.True(default(Track) == default(Track));
        }

        [Fact]
        public void AMixerPassedByValue_IsStillTheSameMixer() {
            MixerApi original = default;
            MixerApi copy = PassedByValue(original);

            Assert.Equal(original, copy);
            Assert.Equal(original.IsValid, copy.IsValid);
        }

        private static MixerApi PassedByValue(MixerApi mixer) {
            return mixer;
        }
    }
}
