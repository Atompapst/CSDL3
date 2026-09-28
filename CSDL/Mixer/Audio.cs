// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Extensions;

namespace CSDL.Mixer {
    /// <summary>
    /// Loaded audio data, ready to be played once via
    /// <see cref="Mixer.Play"/> or assigned to a <see cref="Track"/> for full playback control.
    /// </summary>
    public readonly partial struct Audio {
        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.LoadAudio"/>
        public Audio(Mixer mixer, string path, bool predecode = false)
            : this(LoadFromPath(mixer, path, predecode), HandleKind.Owned) { }

        private static NativePtr<Opaque.SdlAudio> LoadFromPath(Mixer mixer, string path, bool predecode) {
            mixer.ThrowIfInvalid(nameof(mixer));
            Mixer.EnsureInitialized();
            return SDL.LoadAudio(mixer.Handle, path, predecode).ThrowIfInvalid(nameof(Audio));
        }
        
        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.LoadAudio_IO"/>
        public Audio(Mixer mixer, File.IOStream src, bool predecode = false, bool closeAfter = false)
            : this(LoadFromStream(mixer, src, predecode, closeAfter), HandleKind.Owned) { }

        private static NativePtr<Opaque.SdlAudio> LoadFromStream(Mixer mixer, File.IOStream src, bool predecode, bool closeAfter) {
            src.ThrowIfInvalid(nameof(src));
            Mixer.EnsureInitialized();
            NativePtr<Opaque.SdlAudio> audio = SDL.LoadAudio_IO(MixerHandle(mixer), src.Handle, predecode, closeAfter);
            ReleaseStream(src, closeAfter);
            return audio.ThrowIfInvalid(nameof(Audio));
        }

        /// <param name="properties">how to load the audio. <see cref="AudioLoadProperties.IOStream"/> is required.</param>
        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.LoadAudioWithProperties"/>
        public static Audio Load(AudioLoadProperties properties) {
            ArgumentNullException.ThrowIfNull(properties);
            Mixer.EnsureInitialized();
            try {
                return new Audio(SDL.LoadAudioWithProperties(properties.Handle).ThrowIfInvalid(), HandleKind.Owned);
            } finally {
                properties.CompleteLoad();
            }
        }
        
        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.LoadAudioNoCopy"/>
        public static Audio LoadNoCopy(Mixer mixer, IntPtr data, nuint length, bool freeWhenDone = false) {
            Mixer.EnsureInitialized();
            return new Audio(SDL.LoadAudioNoCopy(MixerHandle(mixer), data, length, freeWhenDone).ThrowIfInvalid(), HandleKind.Owned);
        }
        
        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.LoadRawAudio"/>
        public static Audio LoadRaw(Mixer mixer, ReadOnlySpan<byte> data, CSDL.Audio.AudioSpec spec) {
            if (data.IsEmpty) {
                throw new ArgumentException("Raw PCM data cannot be empty.", nameof(data));
            }
            Mixer.EnsureInitialized();
            unsafe {
                fixed (byte* ptr = data) {
                    return new Audio(SDL.LoadRawAudio(MixerHandle(mixer), (IntPtr)ptr, (nuint)data.Length, in spec).ThrowIfInvalid(), HandleKind.Owned);
                }
            }
        }
        
        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.LoadRawAudio_IO"/>
        public static Audio LoadRaw(Mixer mixer, File.IOStream src, CSDL.Audio.AudioSpec spec, bool closeAfter = false) {
            src.ThrowIfInvalid(nameof(src));
            Mixer.EnsureInitialized();
            NativePtr<Opaque.SdlAudio> audio = SDL.LoadRawAudio_IO(MixerHandle(mixer), src.Handle, in spec, closeAfter);
            ReleaseStream(src, closeAfter);
            return new Audio(audio.ThrowIfInvalid(), HandleKind.Owned);
        }
        
        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.LoadRawAudioNoCopy"/>
        public static Audio LoadRawNoCopy(Mixer mixer, IntPtr data, nuint length, CSDL.Audio.AudioSpec spec, bool freeWhenDone = false) {
            Mixer.EnsureInitialized();
            return new Audio(SDL.LoadRawAudioNoCopy(MixerHandle(mixer), data, length, in spec, freeWhenDone).ThrowIfInvalid(), HandleKind.Owned);
        }
        
        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.CreateSineWaveAudio"/>
        public static Audio CreateSineWave(Mixer mixer, int hz, float amplitude, long ms) {
            Mixer.EnsureInitialized();
            return new Audio(SDL.CreateSineWaveAudio(MixerHandle(mixer), hz, amplitude, ms).ThrowIfInvalid(), HandleKind.Owned);
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

        // The zero handle is legal here: the mixer only hints at the format the audio is most likely
        // mixed at. A destroyed mixer still throws.
        private static NativePtr<Opaque.SdlMixer> MixerHandle(Mixer mixer) {
            return mixer.IsDefault ? NativePtr<Opaque.SdlMixer>.Zero : mixer.Handle;
        }

        internal static void ReleaseStream(File.IOStream src, bool closeAfter) {
            if (closeAfter) {
                src.Invalidate();
            }
        }
    }
}
