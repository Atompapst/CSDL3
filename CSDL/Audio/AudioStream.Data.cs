// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Extensions;
using System;

namespace CSDL.Audio {
    public readonly partial struct AudioStream {
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
    }
}
