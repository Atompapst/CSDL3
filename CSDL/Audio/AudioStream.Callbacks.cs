// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Extensions;
using System;

namespace CSDL.Audio {
    public readonly partial struct AudioStream {
        /// <summary>
        /// Registers a managed callback invoked when data is requested from the stream (e.g. to feed additional data on demand).
        /// </summary>
        /// <param name="callback">The callback to invoke when the stream needs more data.</param>
        /// <param name="userdata">An optional user data object passed through to <paramref name="callback"/>.</param>
        /// <seealso cref="CSDL.Internal.Docs.Audio.SetAudioStreamGetCallback">SetAudioStreamGetCallback</seealso>
        public bool SetGetCallback(AudioStreamCallback callback, object? userdata = null) {
            ArgumentNullException.ThrowIfNull(callback);

            NativePtr<Opaque.SdlAudioStream> handle = Handle;
            SDL_AudioStreamCallbackNative native = AudioStreamCallbackWrapper.Create(callback);
            return Install(
                SDL.SetAudioStreamGetCallback, handle, native, callback, userdata,
                GetCallbackIdFor(handle.Ptr));
        }

        /// <summary>Removes the stream's get callback.</summary>
        public bool ClearGetCallback() {
            NativePtr<Opaque.SdlAudioStream> handle = Handle;
            lock (CallbackLock) {
                bool ok = SDL.SetAudioStreamGetCallback(handle, null!, IntPtr.Zero).LogIfFalse();
                if (ok) Unregister(GetCallbackIdFor(handle.Ptr));
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

            NativePtr<Opaque.SdlAudioStream> handle = Handle;
            SDL_AudioStreamCallbackNative native = AudioStreamCallbackWrapper.Create(callback);
            return Install(
                SDL.SetAudioStreamPutCallback, handle, native, callback, userdata,
                PutCallbackIdFor(handle.Ptr));
        }

        /// <summary>Removes the stream's put callback.</summary>
        public bool ClearPutCallback() {
            NativePtr<Opaque.SdlAudioStream> handle = Handle;
            lock (CallbackLock) {
                bool ok = SDL.SetAudioStreamPutCallback(handle, null!, IntPtr.Zero).LogIfFalse();
                if (ok) Unregister(PutCallbackIdFor(handle.Ptr));
                return ok;
            }
        }

        /// <summary>
        ///     Hands <paramref name="native"/> to SDL and, if it takes it, files the registration under
        ///     <paramref name="id"/> - replacing whatever was there.
        /// </summary>
        /// <remarks>
        ///     The registration is made under a throwaway id first and renamed afterwards, because
        ///     <paramref name="id"/> is still held by the callback SDL is about to replace and
        ///     <see cref="CallbackRegistry.Register"/> refuses a duplicate. Freeing the old one first
        ///     would release its userdata while SDL could still be calling it.
        /// </remarks>
        private static bool Install(
            Func<NativePtr<Opaque.SdlAudioStream>, SDL_AudioStreamCallbackNative, IntPtr, CBool> set,
            NativePtr<Opaque.SdlAudioStream> handle,
            SDL_AudioStreamCallbackNative native,
            AudioStreamCallback callback,
            object? userdata,
            string id) {
            string staging = $"{id}:pending:{Guid.NewGuid()}";
            (IntPtr functionPtr, IntPtr userdataPtr) res =
                CallbackRegistry.Register(staging, callback, native, userdata);

            lock (CallbackLock) {
                if (!set(handle, native, res.userdataPtr).LogIfFalse()) {
                    Unregister(staging);
                    return false;
                }

                Unregister(id);
                return CallbackRegistry.UpdateId<AudioStreamCallback, SDL_AudioStreamCallbackNative>(staging, id);
            }
        }

        /// <summary>
        ///     Files a registration SDL already holds under this stream's get-callback id.
        /// </summary>
        /// <remarks>
        ///     <c>SDL_OpenAudioDeviceStream</c> takes the callback and returns the stream in one call,
        ///     so the id has to exist before there is a pointer to name it after. The device opener
        ///     mints a throwaway one and hands it here to be renamed.
        /// </remarks>
        internal void AdoptGetCallbackRegistration(string stagingId) {
            lock (CallbackLock) {
                string id = GetCallbackIdFor(Handle.Ptr);
                Unregister(id);
                CallbackRegistry.UpdateId<AudioStreamCallback, SDL_AudioStreamCallbackNative>(stagingId, id);
            }
        }

        /// <inheritdoc cref="AdoptGetCallbackRegistration"/>
        internal void AdoptPutCallbackRegistration(string stagingId) {
            lock (CallbackLock) {
                string id = PutCallbackIdFor(Handle.Ptr);
                Unregister(id);
                CallbackRegistry.UpdateId<AudioStreamCallback, SDL_AudioStreamCallbackNative>(stagingId, id);
            }
        }
    }
}
