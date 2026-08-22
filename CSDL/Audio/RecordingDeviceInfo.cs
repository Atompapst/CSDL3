// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Extensions;
namespace CSDL.Audio {
    public sealed class RecordingDeviceInfo {
        internal RecordingDeviceInfo(uint id, ulong timestamp) {
            Id = id;
            LastTimestampNs = timestamp;
            Name = GetName(id) ?? "Unknown Device";

            if (TryGetSpec(out AudioSpec spec, out int frames)) {
                PreferredSpec = spec;
                PreferredFrames = frames;
            }
        }

        public uint Id { get; }
        public string Name { get; }
        public ulong LastTimestampNs { get; internal set; }
        public AudioSpec PreferredSpec { get; }
        public int PreferredFrames { get; }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.IsAudioDevicePlayback"/>
        public bool IsPlayback => SDL.IsAudioDevicePlayback(Id);
        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.IsAudioDevicePhysical"/>
        public bool IsPhysical => SDL.IsAudioDevicePhysical(Id);

        public RecordingDevice Open() {
            return Open(PreferredSpec);
        }

        public RecordingDevice Open(AudioFormat format, int frequency, int channels) {
            return Open(new AudioSpec(format, frequency, channels));
        }

        public RecordingDevice Open(AudioSpec desiredSpec) {
            uint logicalId = SDL.OpenAudioDevice(Id, desiredSpec);
            if (logicalId == 0) {
                Error.Throw(nameof(SDL.OpenAudioDevice));
            }

            AudioStream stream = new AudioStream(desiredSpec, desiredSpec);
            if (!SDL.BindAudioStream(logicalId, stream.Handle)) {
                stream.Dispose();
                SDL.CloseAudioDevice(logicalId);
                Error.Throw(nameof(SDL.BindAudioStream));
            }

            stream.ResumeDevice();
            return new RecordingDevice(logicalId, Id, desiredSpec, stream);
        }

        public RecordingDevice Open(AudioSpec desiredSpec, AudioStreamCallback callback, object? userdata = null) {
            return RecordingDevice.OpenFromDeviceStream(Id, desiredSpec, callback, userdata);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.GetAudioDeviceName"/>
        private static string GetName(uint id) {
            return SDL.GetAudioDeviceName(id).ToUtf8StringOrLog() ?? "Unknown Audio Device";
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.GetAudioDeviceFormat"/>
        private bool TryGetSpec(out AudioSpec spec, out int frames) {
            return SDL.GetAudioDeviceFormat(Id, out spec, out frames).LogIfFalse();
        }

        public override string ToString() {
            return $"[Rec] {Name} (ID: {Id})";
        }
    }
}
