// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Extensions;

namespace CSDL.Mixer {
    /// <summary>
    /// Loaded (and optionally predecoded) audio data, ready to be played once via
    /// <see cref="Mixer.Play"/> or assigned to a <see cref="Track"/> for full playback control.
    /// </summary>
    public sealed class Audio : NativeHandle<Opaque.SdlAudio> {
        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.LoadAudio"/>
        public Audio(Mixer mixer, string path, bool predecode = false) {
            ArgumentNullException.ThrowIfNull(mixer);
            Mixer.EnsureInitialized();
            Handle = SDL.LoadAudio(mixer.Handle, path, predecode);
            if (!IsValid) {
                Error.ThrowIfError(nameof(Audio));
            }
        }

        /// <param name="mixer">the mixer this audio is intended for, or <see langword="null"/> to let SDL_mixer pick reasonable defaults.</param>
        /// <param name="src">the stream to load from. It must be seekable.</param>
        /// <param name="predecode">if true, the data is decompressed during load instead of during playback.</param>
        /// <param name="closeAfter">if true, SDL_mixer closes <paramref name="src"/> before returning, success or failure.</param>
        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.LoadAudio_IO"/>
        public Audio(Mixer? mixer, File.IOStream src, bool predecode = false, bool closeAfter = false) {
            ArgumentNullException.ThrowIfNull(src);
            Mixer.EnsureInitialized();
            Handle = SDL.LoadAudio_IO(MixerHandle(mixer), src.Handle, predecode, closeAfter);
            ReleaseStream(src, closeAfter);
            if (!IsValid) {
                Error.ThrowIfError(nameof(Audio));
            }
        }

        internal Audio(NativePtr<Opaque.SdlAudio> handle, bool ownsHandle = true) : base(handle, ownsHandle) { }

        /// <param name="properties">how to load the audio. <see cref="AudioLoadProperties.IOStream"/> is required.</param>
        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.LoadAudioWithProperties"/>
        public static Audio Load(AudioLoadProperties properties) {
            ArgumentNullException.ThrowIfNull(properties);
            Mixer.EnsureInitialized();
            try {
                return new Audio(SDL.LoadAudioWithProperties(properties.Handle).ThrowIfInvalid());
            } finally {
                properties.CompleteLoad();
            }
        }

        /// <param name="mixer">the mixer this audio is intended for, or <see langword="null"/> to let SDL_mixer pick reasonable defaults.</param>
        /// <param name="data">the buffer the audio data lives in, in any supported file format. It must stay alive and pinned for as long as the returned audio does.</param>
        /// <param name="length">the size of <paramref name="data"/>, in bytes.</param>
        /// <param name="freeWhenDone">if true, SDL_mixer hands <paramref name="data"/> to <c>SDL_free</c> once the returned audio is destroyed - so only for buffers that came from SDL's allocator.</param>
        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.LoadAudioNoCopy"/>
        public static Audio LoadNoCopy(Mixer? mixer, IntPtr data, nuint length, bool freeWhenDone = false) {
            Mixer.EnsureInitialized();
            return new Audio(SDL.LoadAudioNoCopy(MixerHandle(mixer), data, length, freeWhenDone).ThrowIfInvalid());
        }

        /// <param name="mixer">the mixer this audio is intended for, or <see langword="null"/> to let SDL_mixer pick reasonable defaults.</param>
        /// <param name="data">the raw PCM data to load. SDL_mixer copies it, so the buffer is free to go afterwards.</param>
        /// <param name="spec">the format <paramref name="data"/> is in.</param>
        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.LoadRawAudio"/>
        public static Audio LoadRaw(Mixer? mixer, ReadOnlySpan<byte> data, CSDL.Audio.AudioSpec spec) {
            if (data.IsEmpty) {
                throw new ArgumentException("Raw PCM data cannot be empty.", nameof(data));
            }
            Mixer.EnsureInitialized();
            unsafe {
                fixed (byte* ptr = data) {
                    return new Audio(SDL.LoadRawAudio(MixerHandle(mixer), (IntPtr)ptr, (nuint)data.Length, in spec).ThrowIfInvalid());
                }
            }
        }

        /// <param name="mixer">the mixer this audio is intended for, or <see langword="null"/> to let SDL_mixer pick reasonable defaults.</param>
        /// <param name="src">the stream to load the raw PCM data from.</param>
        /// <param name="spec">the format the data is in.</param>
        /// <param name="closeAfter">if true, SDL_mixer closes <paramref name="src"/> before returning, success or failure.</param>
        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.LoadRawAudio_IO"/>
        public static Audio LoadRaw(Mixer? mixer, File.IOStream src, CSDL.Audio.AudioSpec spec, bool closeAfter = false) {
            ArgumentNullException.ThrowIfNull(src);
            Mixer.EnsureInitialized();
            NativePtr<Opaque.SdlAudio> audio = SDL.LoadRawAudio_IO(MixerHandle(mixer), src.Handle, in spec, closeAfter);
            ReleaseStream(src, closeAfter);
            return new Audio(audio.ThrowIfInvalid());
        }

        /// <param name="mixer">the mixer this audio is intended for, or <see langword="null"/> to let SDL_mixer pick reasonable defaults.</param>
        /// <param name="data">the buffer the raw PCM data lives in. It must stay alive and pinned for as long as the returned audio does.</param>
        /// <param name="length">the size of <paramref name="data"/>, in bytes.</param>
        /// <param name="spec">the format the data is in.</param>
        /// <param name="freeWhenDone">if true, SDL_mixer hands <paramref name="data"/> to <c>SDL_free</c> once the returned audio is destroyed - so only for buffers that came from SDL's allocator.</param>
        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.LoadRawAudioNoCopy"/>
        public static Audio LoadRawNoCopy(Mixer? mixer, IntPtr data, nuint length, CSDL.Audio.AudioSpec spec, bool freeWhenDone = false) {
            Mixer.EnsureInitialized();
            return new Audio(SDL.LoadRawAudioNoCopy(MixerHandle(mixer), data, length, in spec, freeWhenDone).ThrowIfInvalid());
        }

        /// <param name="mixer">the mixer this audio is intended for. May be <see langword="null"/>.</param>
        /// <param name="hz">the sinewave's frequency in Hz, which decides its pitch.</param>
        /// <param name="amplitude">the sinewave's amplitude from 0.0f (silent) to 1.0f (very loud).</param>
        /// <param name="ms">how many milliseconds of audio to generate, or less than zero for infinite audio.</param>
        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.CreateSineWaveAudio"/>
        public static Audio CreateSineWave(Mixer? mixer, int hz, float amplitude, long ms) {
            Mixer.EnsureInitialized();
            return new Audio(SDL.CreateSineWaveAudio(MixerHandle(mixer), hz, amplitude, ms).ThrowIfInvalid());
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.GetAudioProperties"/>
        public AudioProperties? Properties {
            get {
                uint id = SDL.GetAudioProperties(Handle);
                if (id == 0) {
                    Error.LogError(nameof(SDL.GetAudioProperties));
                    return null;
                }
                return new AudioProperties(id);
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.GetAudioDuration"/>
        public long DurationFrames => SDL.GetAudioDuration(Handle);

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.GetAudioFormat"/>
        public bool GetFormat(out CSDL.Audio.AudioSpec spec) {
            bool ok = SDL.GetAudioFormat(Handle, out spec);
            if (!ok) {
                Error.LogError(nameof(GetFormat));
            }
            return ok;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.AudioFramesToMS"/>
        public long FramesToMS(long frames) {
            return SDL.AudioFramesToMS(Handle, frames);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.AudioMSToFrames"/>
        public long MSToFrames(long ms) {
            return SDL.AudioMSToFrames(Handle, ms);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.DestroyAudio"/>
        protected override void DisposeResource() {
            SDL.DestroyAudio(Handle);
        }

        // A null mixer is legal here: it only hints at the format the audio is most likely mixed at.
        internal static NativePtr<Opaque.SdlMixer> MixerHandle(Mixer? mixer) {
            return mixer?.Handle ?? NativePtr<Opaque.SdlMixer>.Zero;
        }

        // SDL_mixer closed the stream itself, so prevent IOStream.Dispose from closing it again.
        internal static void ReleaseStream(File.IOStream src, bool closeAfter) {
            if (closeAfter) {
                src.Handle = NativePtr<Opaque.SdlIOStream>.Zero;
            }
        }
    }
}
