// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Extensions;
namespace CSDL.Audio {
    public abstract class AudioDeviceBase : NativeHandle<uint> {
        private bool _closed;
        private readonly AudioStream _stream;
        private readonly bool _deviceOwnedByStream;

        internal AudioDeviceBase(uint logicalId, uint sourceDeviceId, AudioSpec spec, AudioStream stream, bool deviceOwnedByStream = false) {
            Handle = new NativePtr<uint>((nint)logicalId);
            Id = logicalId;
            SourceDeviceId = sourceDeviceId;
            Spec = spec;
            _stream = stream;
            _deviceOwnedByStream = deviceOwnedByStream;
            GetFormat(out _, out _);
        }

        public uint Id { get; }
        public uint SourceDeviceId { get; }
        public AudioSpec Spec { get; private set; }
        public int SampleFrames { get; private set; }
        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.IsAudioDevicePhysical"/>
        public bool IsPhysical => SDL.IsAudioDevicePhysical(Id);
        public bool IsLogical => !IsPhysical;
        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.AudioDevicePaused"/>
        public bool Paused => SDL.AudioDevicePaused(Id);

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.SetAudioDeviceGain"/>
        public float Gain {
            get => GetGain(Id);
            set => SetGain(Id, value);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.GetAudioDeviceChannelMap"/>
        public NativePtr<int> ChannelMap(out int count) {
            return SDL.GetAudioDeviceChannelMap(Id, out count);
        }

        /// <summary>Gets a managed copy of the device channel map, or an empty array for SDL's default mapping.</summary>
        public int[] GetChannelMap() {
            NativePtr<int> channelMap = ChannelMap(out int count);
            try {
                return channelMap.IsNull || count <= 0 ? System.Array.Empty<int>() : channelMap.ToManaged(count);
            } finally {
                channelMap.Free();
            }
        }

        /// <summary>Gets the device format SDL actually selected for this logical device.</summary>
        public bool GetFormat(out AudioSpec spec, out int sampleFrames) {
            bool ok = SDL.GetAudioDeviceFormat(Id, out spec, out sampleFrames).LogIfFalse();
            if (ok) {
                Spec = spec;
                SampleFrames = sampleFrames;
            }
            return ok;
        }

        internal AudioStream Stream => _stream;

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.BindAudioStreams"/>
        public bool BindStreams(AudioStream[] streams) {
            if (streams == null || streams.Length == 0) return false;

            bool result = false;
            unsafe {
                streams.WithPointers((ptr, count) => {
                    result = SDL.BindAudioStreams(Id, ptr, (int)count).LogIfFalse();
                });
            }
            return result;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.UnbindAudioStreams"/>
        public static void UnbindStreams(AudioStream[] streams) {
            if (streams == null || streams.Length == 0) return;

            unsafe {
                streams.WithPointers((ptr, count) => {
                    SDL.UnbindAudioStreams(ptr, (int)count);
                });
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.PauseAudioDevice"/>
        public bool Pause() {
            return SDL.PauseAudioDevice(Id).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.ResumeAudioDevice"/>
        public bool Resume() {
            return SDL.ResumeAudioDevice(Id).LogIfFalse();
        }

        public void Close() {
            Dispose();
        }

        protected override void DisposeResource() {
            if (_closed) return;

            if (!_deviceOwnedByStream) {
                SDL.CloseAudioDevice(Id);
            }
            _stream.Dispose();
            _closed = true;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.GetAudioDeviceGain"/>
        private static float GetGain(uint id) {
            return SDL.GetAudioDeviceGain(id).LogIfInvalid(-1.0f);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.SetAudioDeviceGain"/>
        private static bool SetGain(uint id, float gain) {
            return SDL.SetAudioDeviceGain(id, gain).LogIfFalse();
        }
    }

}
