// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Extensions;
using System;

namespace CSDL.Audio {
    public class AudioStream : NativeHandle<Opaque.SdlAudioStream> {
        private readonly object _callbackLock = new object();
        private string? _getCallbackId;
        private string? _putCallbackId;
        private int _trackLeaseCount;
        private NativePtr<Opaque.SdlAudioStream> _deferredHandle;

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.CreateAudioStream"/>
        public AudioStream(AudioSpec? srcSpec, AudioSpec? dstSpec) {
            AudioSpec src = srcSpec.GetValueOrDefault();
            AudioSpec dst = dstSpec.GetValueOrDefault();
            unsafe {
                AudioSpec* srcPtr = srcSpec.HasValue ? &src : null;
                AudioSpec* dstPtr = dstSpec.HasValue ? &dst : null;
                Handle = SDL.CreateAudioStreamNullable(srcPtr, dstPtr).ThrowIfInvalid();
            }
            SourceSpec = src;
            DestinationSpec = dst;
        }

        internal AudioStream(IntPtr audioStreamPtr, bool ownsHandle = false) : base(audioStreamPtr, ownsHandle) {
            GetAudioStreamFormat();
        }

        public AudioStreamProperties? Properties => GetAudioStreamProperties();

        public AudioSpec SourceSpec { get; private set; }
        public AudioSpec DestinationSpec { get; private set; }

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

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.PutAudioStreamPlanarData"/>
        public bool PutPlanarData(NativePtr<nint> channelBuffers, int numChannels, int numSamples) {
            return SDL.PutAudioStreamPlanarData(Handle, channelBuffers, numChannels, numSamples).LogIfFalse();
        }

        /// <summary>
        ///     Adds planar audio data to the stream, one pointer per channel.
        /// </summary>
        /// <param name="channelBuffers">One native buffer pointer per channel, each holding <paramref name="numSamples"/> samples.</param>
        /// <param name="numSamples">The number of samples per channel buffer.</param>
        public bool PutPlanarData(IntPtr[] channelBuffers, int numSamples) {
            if (channelBuffers == null || channelBuffers.Length == 0) return false;
            unsafe {
                fixed (IntPtr* ptr = channelBuffers) {
                    return SDL.PutAudioStreamPlanarData(Handle, (nint*)ptr, channelBuffers.Length, numSamples).LogIfFalse();
                }
            }
        }


        /// <summary>
        /// Adds data to the stream without copying it, invoking a managed callback once SDL is done with the buffer.
        /// </summary>
        /// <param name="data">A pointer to the audio data to add to the stream.</param>
        /// <param name="len">The number of bytes to add to the stream.</param>
        /// <param name="callback">The callback to invoke once SDL no longer needs <paramref name="data"/>.</param>
        /// <param name="userdata">An optional user data object passed through to <paramref name="callback"/>.</param>
        /// <seealso cref="CSDL.Internal.Docs.Audio.PutAudioStreamDataNoCopy">PutAudioStreamDataNoCopy</seealso>
        public bool PutDataNoCopy(IntPtr data, int len, AudioStreamDataCompleteCallback callback, object? userdata = null) {
            ArgumentNullException.ThrowIfNull(callback);

            string id = $"AudioStreamDataComplete:{Guid.NewGuid()}";
            AudioStreamDataCompleteCallback registeredCallback = (userData, buffer, bufferLength) => {
                try {
                    callback(userData, buffer, bufferLength);
                } finally {
                    CallbackRegistry.Unregister<AudioStreamDataCompleteCallback, SDL_AudioStreamDataCompleteCallbackNative>(id);
                }
            };
            SDL_AudioStreamDataCompleteCallbackNative cb = AudioStreamDataCompleteCallbackWrapper.Create(registeredCallback);

            (IntPtr _, IntPtr userdataPtr) res = CallbackRegistry.Register(id, registeredCallback, cb, userdata);
            bool ok = SDL.PutAudioStreamDataNoCopy(Handle, data, len, cb, res.userdataPtr).LogIfFalse();
            if (!ok) {
                CallbackRegistry.Unregister<AudioStreamDataCompleteCallback, SDL_AudioStreamDataCompleteCallbackNative>(id);
            }
            return ok;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.PutAudioStreamDataNoCopy"/>
        public bool PutDataNoCopy(IntPtr data, int len) {
            return SDL.PutAudioStreamDataNoCopy(Handle, data, len, null, IntPtr.Zero).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.PutAudioStreamData"/>
        public bool PutData(byte[] data) {
            if (data == null || data.Length == 0) return true;
            unsafe {
                fixed (byte* d = data) {
                    return SDL.PutAudioStreamData(Handle, (nint)d, data.Length).LogIfFalse();
                }
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.PutAudioStreamData"/>
        public bool PutData(NativePtr<byte> data, int len) {
            if (data == IntPtr.Zero || len <= 0) return true;

            return SDL.PutAudioStreamData(Handle, data, len).LogIfFalse();
        }

        /// Retrieves audio data from the audio stream and stores it in the specified buffer.
        /// <seealso cref="GetData(byte[], int)"/>
        public int GetData(byte[] buffer) {
            if (buffer == null || buffer.Length == 0) return 0;
            return GetData(buffer, buffer.Length);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.GetAudioStreamData"/>
        public int GetData(byte[] buffer, int length) {
            if (buffer == null || buffer.Length == 0 || length <= 0) return 0;
            if (length > buffer.Length) length = buffer.Length;

            unsafe {
                fixed (byte* d = buffer) {
                    return SDL.GetAudioStreamData(Handle, (nint)d, length).LogIfInvalid(-1);
                }
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.GetAudioStreamData"/>
        public int GetData(NativePtr<byte> buffer, int length) {
            if (buffer.IsNull || length <= 0) return 0;

            return SDL.GetAudioStreamData(Handle, buffer, length).LogIfInvalid(-1);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.FlushAudioStream"/>
        public bool Flush() {
            return SDL.FlushAudioStream(Handle).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.ClearAudioStream"/>
        public bool Clear() {
            return SDL.ClearAudioStream(Handle).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.PauseAudioStreamDevice"/>
        public bool PauseDevice() {
            return SDL.PauseAudioStreamDevice(Handle).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.ResumeAudioStreamDevice"/>
        public bool ResumeDevice() {
            return SDL.ResumeAudioStreamDevice(Handle).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.SetAudioStreamFormat"/>
        public bool SetAudioStreamFormat(AudioSpec? srcSpec, AudioSpec? dstSpec) {
            AudioSpec src = srcSpec.GetValueOrDefault();
            AudioSpec dst = dstSpec.GetValueOrDefault();
            bool ok;
            unsafe {
                AudioSpec* srcPtr = srcSpec.HasValue ? &src : null;
                AudioSpec* dstPtr = dstSpec.HasValue ? &dst : null;
                ok = SDL.SetAudioStreamFormatNullable(Handle, srcPtr, dstPtr).LogIfFalse();
            }
            if (ok) {
                if (srcSpec.HasValue) SourceSpec = src;
                if (dstSpec.HasValue) DestinationSpec = dst;
            }
            return ok;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.LockAudioStream"/>
        public AudioStreamLock AcquireLock() {
            if (!SDL.LockAudioStream(Handle).LogIfFalse()) {
                return default;
            }

            return new AudioStreamLock(this);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.LockAudioStream"/>
        public bool Lock() {
            return SDL.LockAudioStream(Handle).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.UnlockAudioStream"/>
        public bool Unlock() {
            return SDL.UnlockAudioStream(Handle).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.UnbindAudioStream"/>
        public void Unbind() {
            SDL.UnbindAudioStream(Handle);
        }

        /// <summary>
        /// Registers a managed callback invoked when data is requested from the stream (e.g. to feed additional data on demand).
        /// </summary>
        /// <param name="callback">The callback to invoke when the stream needs more data.</param>
        /// <param name="userdata">An optional user data object passed through to <paramref name="callback"/>.</param>
        /// <seealso cref="CSDL.Internal.Docs.Audio.SetAudioStreamGetCallback">SetAudioStreamGetCallback</seealso>
        public bool SetGetCallback(AudioStreamCallback callback, object? userdata = null) {
            ArgumentNullException.ThrowIfNull(callback);

            SDL_AudioStreamCallbackNative native = CreateCallback(callback);
            string id = $"AudioStreamGet:{Guid.NewGuid()}";
            (IntPtr functionPtr, IntPtr userdataPtr) res = CallbackRegistry.Register(id, callback, native, userdata);

            lock (_callbackLock) {
                if (!SDL.SetAudioStreamGetCallback(Handle, native, res.userdataPtr).LogIfFalse()) {
                    CallbackRegistry.Unregister<AudioStreamCallback, SDL_AudioStreamCallbackNative>(id);
                    return false;
                }

                UnregisterGetCallback();
                _getCallbackId = id;
                return true;
            }
        }

        /// <summary>Removes the stream's get callback.</summary>
        public bool ClearGetCallback() {
            lock (_callbackLock) {
                bool ok = SDL.SetAudioStreamGetCallback(Handle, null!, IntPtr.Zero).LogIfFalse();
                if (ok) UnregisterGetCallback();
                return ok;
            }
        }

        /// <summary>
        /// Registers a managed callback invoked when data is added to the stream.
        /// </summary>
        /// <param name="callback">The callback to invoke when data is put into the stream.</param>
        /// <param name="userdata">An optional user data object passed through to <paramref name="callback"/>.</param>
        /// <seealso cref="CSDL.Internal.Docs.Audio.SetAudioStreamPutCallback">SetAudioStreamPutCallback</seealso>
        public bool SetPutCallback(AudioStreamCallback callback, object? userdata = null) {
            ArgumentNullException.ThrowIfNull(callback);

            SDL_AudioStreamCallbackNative cb = CreateCallback(callback);
            string id = $"AudioStreamPut:{Guid.NewGuid()}";
            (IntPtr functionPtr, IntPtr userdataPtr) res = CallbackRegistry.Register(id, callback, cb, userdata);

            lock (_callbackLock) {
                if (!SDL.SetAudioStreamPutCallback(Handle, cb, res.userdataPtr).LogIfFalse()) {
                    CallbackRegistry.Unregister<AudioStreamCallback, SDL_AudioStreamCallbackNative>(id);
                    return false;
                }

                UnregisterPutCallback();
                _putCallbackId = id;
                return true;
            }
        }

        /// <summary>Removes the stream's put callback.</summary>
        public bool ClearPutCallback() {
            lock (_callbackLock) {
                bool ok = SDL.SetAudioStreamPutCallback(Handle, null!, IntPtr.Zero).LogIfFalse();
                if (ok) UnregisterPutCallback();
                return ok;
            }
        }

        internal void SetGetCallbackRegistration(string id) {
            lock (_callbackLock) {
                UnregisterGetCallback();
                _getCallbackId = id;
            }
        }

        internal void SetPutCallbackRegistration(string id) {
            lock (_callbackLock) {
                UnregisterPutCallback();
                _putCallbackId = id;
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.DestroyAudioStream"/>
        protected override void DisposeResource() {
            if (System.Threading.Volatile.Read(ref _trackLeaseCount) != 0) {
                _deferredHandle = Handle;
                return;
            }

            lock (_callbackLock) {
                SDL.SetAudioStreamGetCallback(Handle, null!, IntPtr.Zero);
                SDL.SetAudioStreamPutCallback(Handle, null!, IntPtr.Zero);
                UnregisterGetCallback();
                UnregisterPutCallback();
            }
            SDL.DestroyAudioStream(Handle);
        }

        internal void AcquireTrackLease() {
            System.Threading.Interlocked.Increment(ref _trackLeaseCount);
        }

        internal void ReleaseTrackLease() {
            if (System.Threading.Interlocked.Decrement(ref _trackLeaseCount) != 0 || _deferredHandle.IsNull) return;
            SDL.DestroyAudioStream(_deferredHandle);
            _deferredHandle = NativePtr<Opaque.SdlAudioStream>.Zero;
        }

        private void UnregisterGetCallback() {
            if (_getCallbackId is null) {
                return;
            }

            CallbackRegistry.Unregister<AudioStreamCallback, SDL_AudioStreamCallbackNative>(_getCallbackId);
            _getCallbackId = null;
        }

        private void UnregisterPutCallback() {
            if (_putCallbackId is null) {
                return;
            }

            CallbackRegistry.Unregister<AudioStreamCallback, SDL_AudioStreamCallbackNative>(_putCallbackId);
            _putCallbackId = null;
        }

        private SDL_AudioStreamCallbackNative CreateCallback(AudioStreamCallback callback) {
            return (userdataPtr, _, additionalAmount, totalAmount) => {
                try {
                    callback(CallbackRegistry.GetUserdata(userdataPtr), this, additionalAmount, totalAmount);
                } catch (Exception ex) {
                    Log.Error(ex, "Managed audio stream callback threw an exception.");
                }
            };
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.GetAudioStreamFormat"/>
        private bool GetAudioStreamFormat() {
            bool ok = SDL.GetAudioStreamFormat(Handle, out AudioSpec src, out AudioSpec dst).LogIfFalse();
            if (ok) {
                SourceSpec = src;
                DestinationSpec = dst;
            }
            return ok;
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

    public readonly struct AudioStreamLock : IDisposable {
        private readonly AudioStream _stream;

        internal AudioStreamLock(AudioStream stream) {
            _stream = stream;
        }

        public void Dispose() {
            if (_stream != null && !_stream.Handle.IsNull) {
                _stream.Unlock();
            }
        }
    }
}
