// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Extensions;
using System;

namespace CSDL.Audio {
    public readonly partial struct AudioStream {
        public AudioStreamProperties? Properties => GetAudioStreamProperties();

        /// <summary>The format this stream converts from.</summary>
        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.GetAudioStreamFormat"/>
        public AudioSpec SourceSpec {
            get {
                SDL.GetAudioStreamFormat(Handle, out AudioSpec src, out AudioSpec _).LogIfFalse();
                return src;
            }
        }

        /// <summary>The format this stream converts to.</summary>
        /// <inheritdoc cref="SourceSpec"/>
        public AudioSpec DestinationSpec {
            get {
                SDL.GetAudioStreamFormat(Handle, out AudioSpec _, out AudioSpec dst).LogIfFalse();
                return dst;
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.GetAudioStreamFormat"/>
        public bool GetFormat(out AudioSpec source, out AudioSpec destination) {
            return SDL.GetAudioStreamFormat(Handle, out source, out destination).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.GetAudioStreamAvailable"/>
        public int Available => SDL.GetAudioStreamAvailable(Handle);
        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.GetAudioStreamQueued"/>
        public int Queued => SDL.GetAudioStreamQueued(Handle);
        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.AudioStreamDevicePaused"/>
        public bool IsDevicePaused => SDL.AudioStreamDevicePaused(Handle);
        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.GetAudioStreamDevice"/>
        public uint DeviceId => SDL.GetAudioStreamDevice(Handle);

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.SetAudioStreamFrequencyRatio"/>
        public float FrequencyRatio {
            get => GetAudioStreamFrequencyRatio();
            set => SetAudioStreamFrequencyRatio(value);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.SetAudioStreamGain"/>
        public float Gain {
            get => GetAudioStreamGain();
            set => SetAudioStreamGain(value);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.GetAudioStreamInputChannelMap"/>
        public NativePtr<int> GetInputChannelMap(out int count) {
            return SDL.GetAudioStreamInputChannelMap(Handle, out count);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.GetAudioStreamOutputChannelMap"/>
        public NativePtr<int> GetOutputChannelMap(out int count) {
            return SDL.GetAudioStreamOutputChannelMap(Handle, out count);
        }

        public int[] GetInputChannelMapArray() {
            NativePtr<int> native = GetInputChannelMap(out int count);
            try {
                return native.IsNull || count <= 0 ? Array.Empty<int>() : native.ToManaged(count);
            } finally {
                native.Free();
            }
        }

        public int[] GetOutputChannelMapArray() {
            NativePtr<int> native = GetOutputChannelMap(out int count);
            try {
                return native.IsNull || count <= 0 ? Array.Empty<int>() : native.ToManaged(count);
            } finally {
                native.Free();
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.SetAudioStreamInputChannelMap"/>
        public void SetInputChannelMap(int[]? map) {
            if (map is null) {
                SDL.SetAudioStreamInputChannelMap(Handle, NativePtr<int>.Zero, 0).LogIfFalse();
            } else {
                SDL.SetAudioStreamInputChannelMap(Handle, map.AsSpan(), map.Length).LogIfFalse();
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.SetAudioStreamOutputChannelMap"/>
        public void SetOutputChannelMap(int[]? map) {
            if (map is null) {
                SDL.SetAudioStreamOutputChannelMap(Handle, NativePtr<int>.Zero, 0).LogIfFalse();
            } else {
                SDL.SetAudioStreamOutputChannelMap(Handle, map.AsSpan(), map.Length).LogIfFalse();
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.GetAudioStreamProperties"/>
        private AudioStreamProperties? GetAudioStreamProperties() {
            uint id = SDL.GetAudioStreamProperties(Handle);
            if (id == 0) {
                Error.LogError(nameof(SDL.GetAudioStreamProperties));
                return null;
            }

            return new AudioStreamProperties(id);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.GetAudioStreamFrequencyRatio"/>
        private float GetAudioStreamFrequencyRatio() {
            return SDL.GetAudioStreamFrequencyRatio(Handle).LogIfInvalid(0.0f);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.SetAudioStreamFrequencyRatio"/>
        private bool SetAudioStreamFrequencyRatio(float ratio) {
            return SDL.SetAudioStreamFrequencyRatio(Handle, ratio).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.GetAudioStreamGain"/>
        private float GetAudioStreamGain() {
            return SDL.GetAudioStreamGain(Handle).LogIfInvalid(-1.0f);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.SetAudioStreamGain"/>
        private bool SetAudioStreamGain(float gain) {
            return SDL.SetAudioStreamGain(Handle, gain).LogIfFalse();
        }
    }
}
