// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Extensions;
using System;

namespace CSDL.Audio {
    internal struct AudioDeviceCore {
        private bool _closed;

        internal AudioDeviceCore(uint logicalId, uint sourceDeviceId, AudioSpec spec, AudioStream stream, bool deviceOwnedByStream) {
            Id = logicalId;
            SourceDeviceId = sourceDeviceId;
            Spec = spec;
            Stream = stream;
            DeviceOwnedByStream = deviceOwnedByStream;
            SampleFrames = 0;
            _closed = false;
            GetFormat(out _, out _);
        }

        /// <summary>The logical device id SDL opened for this wrapper.</summary>
        internal readonly uint Id;

        /// <summary>The physical device the logical one was opened from.</summary>
        internal readonly uint SourceDeviceId;

        /// <summary>The stream bound to this device, which is disposed together with it.</summary>
        internal readonly AudioStream Stream;

        /// <summary>True when SDL closes the device along with the stream, so we must not close it.</summary>
        internal readonly bool DeviceOwnedByStream;

        /// <summary>The format SDL actually selected, refreshed by <see cref="GetFormat" />.</summary>
        internal AudioSpec Spec;

        /// <summary>The device buffer size SDL actually selected, in sample frames.</summary>
        internal int SampleFrames;

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.IsAudioDevicePhysical" />
        internal readonly bool IsPhysical => SDL.IsAudioDevicePhysical(Id);

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.AudioDevicePaused" />
        internal readonly bool Paused => SDL.AudioDevicePaused(Id);

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.GetAudioDeviceGain" />
        internal readonly float GetGain() {
            return SDL.GetAudioDeviceGain(Id).LogIfInvalid(-1.0f);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.SetAudioDeviceGain" />
        internal readonly bool SetGain(float gain) {
            return SDL.SetAudioDeviceGain(Id, gain).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.GetAudioDeviceChannelMap" />
        internal readonly NativePtr<int> ChannelMap(out int count) {
            return SDL.GetAudioDeviceChannelMap(Id, out count);
        }

        /// <summary>Gets a managed copy of the device channel map, or an empty array for SDL's default mapping.</summary>
        internal readonly int[] GetChannelMap() {
            NativePtr<int> channelMap = ChannelMap(out int count);
            try {
                return channelMap.IsNull || count <= 0 ? System.Array.Empty<int>() : channelMap.ToManaged(count);
            } finally {
                channelMap.Free();
            }
        }

        /// <summary>Gets the device format SDL actually selected for this logical device.</summary>
        internal bool GetFormat(out AudioSpec spec, out int sampleFrames) {
            bool ok = SDL.GetAudioDeviceFormat(Id, out spec, out sampleFrames).LogIfFalse();
            if (ok) {
                Spec = spec;
                SampleFrames = sampleFrames;
            }
            return ok;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.PauseAudioDevice" />
        internal readonly bool Pause() {
            return SDL.PauseAudioDevice(Id).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.ResumeAudioDevice" />
        internal readonly bool Resume() {
            return SDL.ResumeAudioDevice(Id).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.BindAudioStreams" />
        internal readonly bool BindStreams(AudioStream[] streams) {
            return BindStreams((ReadOnlySpan<AudioStream>)streams);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.BindAudioStreams" />
        internal readonly bool BindStreams(ReadOnlySpan<AudioStream> streams) {
            if (streams.Length == 0) return false;

            uint id = Id;
            bool result = false;
            unsafe {
                streams.WithPointers((ptr, count) => {
                    result = SDL.BindAudioStreams(id, ptr, (int)count).LogIfFalse();
                });
            }
            return result;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.UnbindAudioStreams" />
        internal static void UnbindStreams(AudioStream[]? streams) {
            UnbindStreams((ReadOnlySpan<AudioStream>)streams);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.UnbindAudioStreams" />
        internal static void UnbindStreams(ReadOnlySpan<AudioStream> streams) {
            if (streams.Length == 0) return;

            unsafe {
                streams.WithPointers((ptr, count) => {
                    SDL.UnbindAudioStreams(ptr, (int)count);
                });
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.CloseAudioDevice" />
        /// <remarks>Closing twice is a no-op, as is closing a device SDL owns through its stream.</remarks>
        internal void Close() {
            if (_closed) return;
            _closed = true;

            if (!DeviceOwnedByStream) {
                SDL.CloseAudioDevice(Id);
            }
            Stream.Dispose();
        }
    }
}
