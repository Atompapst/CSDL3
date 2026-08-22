// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Extensions;

namespace CSDL.Mixer {
    /// <summary>
    /// Decodes audio data on demand into a caller-supplied buffer, without playing it and without
    /// needing a <see cref="Mixer"/> at all - useful for turning a file into raw PCM data directly.
    /// </summary>
    public sealed class AudioDecoder : NativeHandle<Opaque.SdlAudioDecoder> {
        static AudioDecoder() {
            Mixer.EnsureInitialized();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.CreateAudioDecoder"/>
        public AudioDecoder(string path, PropertiesID props = default) {
            Handle = SDL.CreateAudioDecoder(path, props).ThrowIfInvalid();
        }

        /// <param name="src">the stream to decode from.</param>
        /// <param name="closeAfter">if true, SDL_mixer closes <paramref name="src"/> when the decoder is done with it - including when this constructor fails.</param>
        /// <param name="props">decoder-specific properties, e.g. from <see cref="AudioLoadProperties"/>. May be left at zero.</param>
        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.CreateAudioDecoder_IO"/>
        public AudioDecoder(File.IOStream src, bool closeAfter = false, PropertiesID props = default) {
            ArgumentNullException.ThrowIfNull(src);
            NativePtr<Opaque.SdlAudioDecoder> decoder = SDL.CreateAudioDecoder_IO(src.Handle, closeAfter, props);
            Audio.ReleaseStream(src, closeAfter);
            Handle = decoder.ThrowIfInvalid();
        }

        /// <remarks>
        /// The file metadata here is the same set <see cref="Audio.Properties"/> exposes.
        /// </remarks>
        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.GetAudioDecoderProperties"/>
        public AudioProperties? Properties {
            get {
                uint id = SDL.GetAudioDecoderProperties(Handle);
                if (id == 0) {
                    Error.LogError(nameof(SDL.GetAudioDecoderProperties));
                    return null;
                }
                return new AudioProperties(id);
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.GetNumAudioDecoders"/>
        public static int Count => SDL.GetNumAudioDecoders();

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.GetAudioDecoder"/>
        public static string? GetName(int index) {
            return SDL.GetAudioDecoder(index).ToUtf8StringOrLog();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.GetAudioDecoderFormat"/>
        public bool GetFormat(out CSDL.Audio.AudioSpec spec) {
            return SDL.GetAudioDecoderFormat(Handle, out spec).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.DecodeAudio"/>
        public int Decode(byte[] buffer, CSDL.Audio.AudioSpec spec) {
            unsafe {
                fixed (byte* ptr = buffer) {
                    return SDL.DecodeAudio(Handle, (IntPtr)ptr, buffer.Length, in spec).LogIfInvalid(-1);
                }
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Mixer.DestroyAudioDecoder"/>
        protected override void DisposeResource() {
            SDL.DestroyAudioDecoder(Handle);
        }
    }
}
