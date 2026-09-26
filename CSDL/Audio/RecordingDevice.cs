// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Extensions;

namespace CSDL.Audio {
    public sealed class RecordingDevice : IDisposable {
        internal RecordingDevice(uint logicalId, uint sourceDeviceId, AudioSpec spec, AudioStream stream)
            : this(logicalId, sourceDeviceId, spec, stream, false) { }

        internal RecordingDevice(uint logicalId, uint sourceDeviceId, AudioSpec spec, AudioStream stream, bool deviceOwnedByStream) {
            _core = new AudioDeviceCore(logicalId, sourceDeviceId, spec, stream, deviceOwnedByStream);
        }

        private AudioDeviceCore _core;
        private int _disposed;

        /// <summary>The logical device id SDL opened for this device.</summary>
        public uint Id => _core.Id;

        /// <summary>The physical device this logical one was opened from.</summary>
        public uint SourceDeviceId => _core.SourceDeviceId;

        /// <summary>The format SDL actually selected for this device.</summary>
        public AudioSpec Spec => _core.Spec;

        /// <summary>The device buffer size SDL actually selected, in sample frames.</summary>
        public int SampleFrames => _core.SampleFrames;

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.IsAudioDevicePhysical"/>
        public bool IsPhysical => _core.IsPhysical;

        /// <summary>The opposite of <see cref="IsPhysical"/>.</summary>
        public bool IsLogical => !_core.IsPhysical;

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.AudioDevicePaused"/>
        public bool Paused => _core.Paused;

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.SetAudioDeviceGain"/>
        public float Gain {
            get => _core.GetGain();
            set => _core.SetGain(value);
        }

        internal AudioStream Stream => _core.Stream;

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.GetAudioDeviceChannelMap"/>
        public NativePtr<int> ChannelMap(out int count) {
            return _core.ChannelMap(out count);
        }

        /// <summary>Gets a managed copy of the device channel map, or an empty array for SDL's default mapping.</summary>
        public int[] GetChannelMap() {
            return _core.GetChannelMap();
        }

        /// <summary>Gets the device format SDL actually selected for this logical device.</summary>
        public bool GetFormat(out AudioSpec spec, out int sampleFrames) {
            return _core.GetFormat(out spec, out sampleFrames);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.BindAudioStreams"/>
        public bool BindStreams(AudioStream[] streams) {
            return _core.BindStreams(streams);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.BindAudioStreams"/>
        public bool BindStreams(ReadOnlySpan<AudioStream> streams) {
            return _core.BindStreams(streams);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.UnbindAudioStreams"/>
        public static void UnbindStreams(AudioStream[]? streams) {
            AudioDeviceCore.UnbindStreams(streams);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.UnbindAudioStreams"/>
        public static void UnbindStreams(ReadOnlySpan<AudioStream> streams) {
            AudioDeviceCore.UnbindStreams(streams);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.PauseAudioDevice"/>
        public bool Pause() {
            return _core.Pause();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.ResumeAudioDevice"/>
        public bool Resume() {
            return _core.Resume();
        }

        /// <summary>Closes the device. Equivalent to <see cref="Dispose"/>.</summary>
        public void Close() {
            Dispose();
        }

        static RecordingDevice() {
            Init.InitSubSystem(InitFlags.Audio);
        }

        public int AvailableBytes => Stream.Available;
        public int QueuedBytes => Stream.Queued;

        public float FrequencyRatio {
            get => Stream.FrequencyRatio;
            set => _core.Stream.FrequencyRatio = value;
        }

        public float StreamGain {
            get => Stream.Gain;
            set => _core.Stream.Gain = value;
        }

        public int[] InputChannelMap => Stream.GetInputChannelMapArray();
        public int[] OutputChannelMap => Stream.GetOutputChannelMapArray();

        public int Read(byte[] buffer) {
            if (buffer == null || buffer.Length == 0) return 0;
            return Stream.GetData(buffer);
        }

        public byte[] Read(int byteCount) {
            if (byteCount <= 0) {
                return Array.Empty<byte>();
            }

            byte[] buffer = new byte[byteCount];
            int read = Stream.GetData(buffer, byteCount);

            if (read <= 0) {
                return Array.Empty<byte>();
            }

            if (read == buffer.Length) {
                return buffer;
            }

            byte[] resized = new byte[read];
            Array.Copy(buffer, resized, read);
            return resized;
        }

        public byte[] ReadAllAvailable() {
            int available = AvailableBytes;
            if (available <= 0) {
                return Array.Empty<byte>();
            }

            return Read(available);
        }

        public void Clear() {
            Stream.Clear();
        }

        public void SetInputChannelMap(int[]? map) {
            Stream.SetInputChannelMap(map);
        }

        public void SetOutputChannelMap(int[]? map) {
            Stream.SetOutputChannelMap(map);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.CloseAudioDevice"/>
        public void Dispose() {
            if (System.Threading.Interlocked.Exchange(ref _disposed, 1) != 0) return;
            _core.Close();
            GC.SuppressFinalize(this);
        }

        public override string ToString() {
            return $"[Rec] LogicalDevice {Id} (Source: {SourceDeviceId}, {Spec})";
        }

        /// <summary>
        /// Opens the default recording device with the specified audio specification or a default specification if none is provided.
        /// </summary>
        /// <param name="spec">Optional audio specification to use for the recording device. If null, a default specification is applied.</param>
        /// <returns>A new instance of <see cref="RecordingDevice"/> representing the opened recording device.</returns>
        public static RecordingDevice OpenDefault(AudioSpec? spec = null) {
            AudioSpec desiredSpec = spec ?? ResolveDefaultSpec(Macros.AudioDeviceDefaultRecording);

            uint logicalId = SDL.OpenAudioDevice(Macros.AudioDeviceDefaultRecording, desiredSpec);
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
            return new RecordingDevice(logicalId, Macros.AudioDeviceDefaultRecording, desiredSpec, stream);
        }

        public static RecordingDevice OpenDefault(AudioSpec? spec, AudioStreamCallback callback, object? userdata = null) {
            AudioSpec desiredSpec = spec ?? ResolveDefaultSpec(Macros.AudioDeviceDefaultRecording);
            return OpenFromDeviceStream(Macros.AudioDeviceDefaultRecording, desiredSpec, callback, userdata);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.GetAudioDeviceFormat"/>
        private static AudioSpec ResolveDefaultSpec(uint deviceId) {
            return SDL.GetAudioDeviceFormat(deviceId, out AudioSpec spec, out _).LogIfFalse() ? spec : new AudioSpec();
        }

        internal static RecordingDevice OpenFromDeviceStream(uint sourceDeviceId, AudioSpec spec, AudioStreamCallback callback, object? userdata = null) {
            SDL_AudioStreamCallbackNative native = AudioStreamCallbackWrapper.Create(callback);
            string callbackId = $"RecordingDeviceStream:{Guid.NewGuid()}";
            (IntPtr functionPtr, IntPtr userdataPtr) res = CallbackRegistry.Register(callbackId, callback, native, userdata);

            IntPtr streamHandle = SDL.OpenAudioDeviceStream(sourceDeviceId, spec, native, res.userdataPtr);
            if (streamHandle == IntPtr.Zero) {
                CallbackRegistry.Unregister<AudioStreamCallback, SDL_AudioStreamCallbackNative>(callbackId);
                Error.Throw(nameof(SDL.OpenAudioDeviceStream));
            }

            AudioStream stream = new AudioStream(streamHandle, true);
            stream.AdoptPutCallbackRegistration(callbackId);
            uint logicalId = stream.DeviceId;

            if (logicalId == 0) {
                stream.Dispose();
                Error.Throw("SDL_GetAudioStreamDevice");
            }

            stream.ResumeDevice();
            return new RecordingDevice(logicalId, sourceDeviceId, spec, stream, true);
        }
    }

}
