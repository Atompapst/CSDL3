// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Extensions;

namespace CSDL.Audio {
    public sealed class PlaybackDevice : AudioDeviceBase {
        private readonly object _postmixCallbackLock = new object();
        private string? _postmixCallbackId;
        internal PlaybackDevice(uint logicalId, uint sourceDeviceId, AudioSpec spec, AudioStream stream)
            : base(logicalId, sourceDeviceId, spec, stream) { }

        internal PlaybackDevice(uint logicalId, uint sourceDeviceId, AudioSpec spec, AudioStream stream, bool deviceOwnedByStream)
            : base(logicalId, sourceDeviceId, spec, stream, deviceOwnedByStream) { }

        static PlaybackDevice() {
            Init.InitSubSystem(InitFlags.Audio);
        }

        public int QueuedBytes => Stream?.Queued ?? 0;
        public int AvailableBytes => Stream?.Available ?? 0;

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

        public void Write(byte[] pcmData) {
            if (pcmData == null || pcmData.Length == 0) return;

            Stream.PutData(pcmData);
            Stream.ResumeDevice();
        }

        public void Write(AudioClip clip) {
            if (clip == null || clip.Handle.IsNull || clip.Length == 0) return;

            Stream.PutData(clip.Handle, (int)clip.Length);
            Stream.ResumeDevice();
        }

        public void Flush() {
            Stream.Flush();
        }

        public void Clear() {
            Stream.Clear();
        }

        public void Stop() {
            Stream.Clear();
            Pause();
        }


        /// <summary>
        /// Registers a managed callback that is invoked with the final mixed audio for this device before it is sent to the hardware.
        /// </summary>
        /// <param name="callback">The callback to invoke with the postmix audio buffer, or <see langword="null"/> to remove any existing callback.</param>
        /// <param name="userdata">An optional user data object passed through to <paramref name="callback"/>.</param>
        /// <seealso cref="CSDL.Internal.Docs.Audio.SetAudioPostmixCallback">SetAudioPostmixCallback</seealso>
        public bool SetPostmixCallback(AudioPostmixCallback callback, object? userdata = null) {
            ArgumentNullException.ThrowIfNull(callback);
            SDL_AudioPostmixCallbackNative native = AudioPostmixCallbackWrapper.Create(callback);
            string id = $"PlaybackPostmix:{Guid.NewGuid()}";
            (IntPtr functionPtr, IntPtr userdataPtr) res = CallbackRegistry.Register(id, callback, native, userdata);
            lock (_postmixCallbackLock) {
                CBool ok = SDL.SetAudioPostmixCallback(Id, native, res.userdataPtr);
                if (!ok) {
                    CallbackRegistry.Unregister<AudioPostmixCallback, SDL_AudioPostmixCallbackNative>(id);
                    return ok.LogIfFalse();
                }
                if (_postmixCallbackId is not null) {
                    CallbackRegistry.Unregister<AudioPostmixCallback, SDL_AudioPostmixCallbackNative>(_postmixCallbackId);
                }
                _postmixCallbackId = id;
                return true;
            }
        }

        /// <summary>Removes the device post-mix callback.</summary>
        public bool ClearPostmixCallback() {
            lock (_postmixCallbackLock) {
                CBool ok = SDL.SetAudioPostmixCallback(Id, null!, IntPtr.Zero);
                if (ok && _postmixCallbackId is not null) {
                    CallbackRegistry.Unregister<AudioPostmixCallback, SDL_AudioPostmixCallbackNative>(_postmixCallbackId);
                    _postmixCallbackId = null;
                }
                return ok.LogIfFalse();
            }
        }

        public void SetInputChannelMap(int[]? map) {
            Stream.SetInputChannelMap(map);
        }

        public void SetOutputChannelMap(int[]? map) {
            Stream.SetOutputChannelMap(map);
        }

        protected override void DisposeResource() {
            ClearPostmixCallback();
            base.DisposeResource();
        }

        public override string ToString() {
            return $"[Play] LogicalDevice {Id} (Source: {SourceDeviceId}, {Spec})";
        }

        /// <summary>
        /// Opens the default playback audio device with the specified audio specification.
        /// </summary>
        /// <param name="spec">
        /// The optional audio specification to use. If null, a default audio specification will be resolved and applied.
        /// </param>
        /// <returns>
        /// A <see cref="PlaybackDevice"/> instance representing the opened default playback device.
        /// </returns>
        /// <seealso cref="Macros.AudioDeviceDefaultPlayback">AudioDeviceDefaultPlayback</seealso>
        public static PlaybackDevice OpenDefault(AudioSpec? spec = null) {
            AudioSpec desiredSpec = spec ?? ResolveDefaultSpec(Macros.AudioDeviceDefaultPlayback);

            uint logicalId = SDL.OpenAudioDevice(Macros.AudioDeviceDefaultPlayback, desiredSpec);
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
            return new PlaybackDevice(logicalId, Macros.AudioDeviceDefaultPlayback, desiredSpec, stream);
        }

        /// <summary>
        /// Opens the default playback audio device with the specified audio specification and callback.
        /// </summary>
        /// <param name="spec">
        /// The optional audio specification to use. If null, a default audio specification will be resolved and applied.
        /// </param>
        /// <param name="callback">
        /// The callback to be invoked for the audio stream handling.
        /// </param>
        /// <param name="userdata">
        /// An optional user data object to pass to the callback. Defaults to null.
        /// </param>
        /// <returns>
        /// A <see cref="PlaybackDevice"/> instance representing the opened default playback device.
        /// </returns>
        /// <seealso cref="Macros.AudioDeviceDefaultPlayback">AudioDeviceDefaultPlayback</seealso>
        public static PlaybackDevice OpenDefault(AudioSpec? spec, AudioStreamCallback callback, object? userdata = null) {
            AudioSpec desiredSpec = spec ?? ResolveDefaultSpec(Macros.AudioDeviceDefaultPlayback);
            return OpenFromDeviceStream(Macros.AudioDeviceDefaultPlayback, desiredSpec, callback, userdata);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.GetAudioDeviceFormat"/>
        private static AudioSpec ResolveDefaultSpec(AudioDeviceID deviceId) {
            return SDL.GetAudioDeviceFormat(deviceId, out AudioSpec raw, out int _).LogIfFalse() ? raw : new AudioSpec();
        }

        internal static PlaybackDevice OpenFromDeviceStream(AudioDeviceID sourceDeviceId, AudioSpec spec, AudioStreamCallback callback, object? userdata = null) {
            AudioStream? stream = null;
            SDL_AudioStreamCallbackNative cb = (userdataPtr, _, additionalAmount, totalAmount) => {
                try {
                    callback(CallbackRegistry.GetUserdata(userdataPtr), stream!, additionalAmount, totalAmount);
                } catch (Exception ex) {
                    Log.Error(ex, "Managed playback callback threw an exception.");
                }
            };
            string callbackId = $"PlaybackDeviceStream:{Guid.NewGuid()}";
            (IntPtr functionPtr, IntPtr userdataPtr) res = CallbackRegistry.Register(callbackId, callback, cb, userdata);

            IntPtr streamHandle = SDL.OpenAudioDeviceStream(sourceDeviceId, spec, cb, res.userdataPtr);
            if (streamHandle == IntPtr.Zero) {
                CallbackRegistry.Unregister<AudioStreamCallback, SDL_AudioStreamCallbackNative>(callbackId);
                Error.Throw(nameof(SDL.OpenAudioDeviceStream));
            }

            stream = new AudioStream(streamHandle, true);
            stream.SetGetCallbackRegistration(callbackId);
            uint logicalId = stream.DeviceId;

            if (logicalId == 0) {
                stream.Dispose();
                Error.Throw("SDL_GetAudioStreamDevice");
            }

            stream.ResumeDevice();
            return new PlaybackDevice(logicalId, sourceDeviceId, spec, stream, true);
        }
    }

}
