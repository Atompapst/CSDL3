// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Extensions;

namespace CSDL.Audio {
    public sealed class PlaybackDevice : IDisposable {
        private readonly object _postmixCallbackLock = new object();
        private string? _postmixCallbackId;

        internal PlaybackDevice(uint logicalId, uint sourceDeviceId, AudioSpec spec, AudioStream stream)
            : this(logicalId, sourceDeviceId, spec, stream, false) { }

        internal PlaybackDevice(uint logicalId, uint sourceDeviceId, AudioSpec spec, AudioStream stream, bool deviceOwnedByStream) {
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

        static PlaybackDevice() {
            Init.InitSubSystem(InitFlags.Audio);
        }

        public int QueuedBytes => Stream.Queued;
        public int AvailableBytes => Stream.Available;

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

        public void Write(byte[] pcmData) {
            if (pcmData == null || pcmData.Length == 0) return;

            Stream.PutData(pcmData);
            Stream.ResumeDevice();
        }

        /// <summary>
        /// Queues a clip for playback, converting it from its own format to the device's.
        /// </summary>
        /// <remarks>
        /// Data already queued keeps the format it was queued with - SDL records it per buffer - so
        /// clips of different formats can be written back to back.
        /// </remarks>
        public void Write(AudioClip clip) {
            if (!clip.IsValid || clip.Length == 0) return;

            Stream.SetAudioStreamFormat(clip.Spec, null);
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

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.CloseAudioDevice"/>
        public void Dispose() {
            if (System.Threading.Interlocked.Exchange(ref _disposed, 1) != 0) return;
            ClearPostmixCallback();
            _core.Close();
            GC.SuppressFinalize(this);
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

            AudioDeviceID logicalId = SDL.OpenAudioDevice(Macros.AudioDeviceDefaultPlayback, desiredSpec);
            if (logicalId.Value == 0) {
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
            SDL_AudioStreamCallbackNative cb = AudioStreamCallbackWrapper.Create(callback);
            string callbackId = $"PlaybackDeviceStream:{Guid.NewGuid()}";
            (IntPtr functionPtr, IntPtr userdataPtr) res = CallbackRegistry.Register(callbackId, callback, cb, userdata);

            IntPtr streamHandle = SDL.OpenAudioDeviceStream(sourceDeviceId, spec, cb, res.userdataPtr);
            if (streamHandle == IntPtr.Zero) {
                CallbackRegistry.Unregister<AudioStreamCallback, SDL_AudioStreamCallbackNative>(callbackId);
                Error.Throw(nameof(SDL.OpenAudioDeviceStream));
            }

            AudioStream stream = new AudioStream(streamHandle, true);
            stream.AdoptGetCallbackRegistration(callbackId);
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
