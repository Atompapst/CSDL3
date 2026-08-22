// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Extensions;

namespace CSDL.Audio {
    public sealed class RecordingDevice : AudioDeviceBase {
        internal RecordingDevice(uint logicalId, uint sourceDeviceId, AudioSpec spec, AudioStream stream)
            : base(logicalId, sourceDeviceId, spec, stream) { }

        internal RecordingDevice(uint logicalId, uint sourceDeviceId, AudioSpec spec, AudioStream stream, bool deviceOwnedByStream)
            : base(logicalId, sourceDeviceId, spec, stream, deviceOwnedByStream) { }

        static RecordingDevice() {
            Init.InitSubSystem(InitFlags.Audio);
        }

        public int AvailableBytes => Stream?.Available ?? 0;
        public int QueuedBytes => Stream?.Queued ?? 0;

        public float FrequencyRatio {
            get => Stream?.FrequencyRatio ?? 1.0f;
            set {
                if (Stream != null) {
                    Stream.FrequencyRatio = value;
                }
            }
        }

        public float StreamGain {
            get => Stream?.Gain ?? 1.0f;
            set {
                if (Stream != null) {
                    Stream.Gain = value;
                }
            }
        }

        // Stream is always set by the constructor (see AudioDeviceBase) and is never re-nulled, so
        // it is accessed directly here rather than via the null-conditional operator.
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
            AudioStream? stream = null;
            SDL_AudioStreamCallbackNative native = (userdataPtr, _, additionalAmount, totalAmount) => {
                try {
                    callback(CallbackRegistry.GetUserdata(userdataPtr), stream!, additionalAmount, totalAmount);
                } catch (Exception ex) {
                    Log.Error(ex, "Managed recording callback threw an exception.");
                }
            };
            string callbackId = $"RecordingDeviceStream:{Guid.NewGuid()}";
            (IntPtr functionPtr, IntPtr userdataPtr) res = CallbackRegistry.Register(callbackId, callback, native, userdata);

            IntPtr streamHandle = SDL.OpenAudioDeviceStream(sourceDeviceId, spec, native, res.userdataPtr);
            if (streamHandle == IntPtr.Zero) {
                CallbackRegistry.Unregister<AudioStreamCallback, SDL_AudioStreamCallbackNative>(callbackId);
                Error.Throw(nameof(SDL.OpenAudioDeviceStream));
            }

            stream = new AudioStream(streamHandle, true);
            stream.SetPutCallbackRegistration(callbackId);
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
