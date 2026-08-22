// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Extensions;

namespace CSDL {
    /// <summary>
    ///     How the app may use Android's external storage.
    /// </summary>
    /// <seealso cref="Android.ExternalStorageState"/>
    [Flags]
    public enum AndroidStorageState : uint {
        /// <summary>External storage is not available at all.</summary>
        None = 0,

        /// <inheritdoc cref="Macros.AndroidExternalStorageRead"/>
        Read = Macros.AndroidExternalStorageRead,

        /// <inheritdoc cref="Macros.AndroidExternalStorageWrite"/>
        Write = Macros.AndroidExternalStorageWrite,
    }

    /// <summary>
    ///     Android-only entry points. On every other platform these fail (or return zero/empty) and set
    ///     an SDL error.
    /// </summary>
    public static class Android {
        /// <inheritdoc cref="CSDL.Internal.Docs.System.GetAndroidActivity"/>
        /// <remarks>
        ///     A JNI local reference to the app's Activity - the caller has to release it with
        ///     <c>DeleteLocalRef</c> through <see cref="JniEnvironment"/>.
        /// </remarks>
        public static nint GetActivity() {
            return SDL.GetAndroidActivity();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.System.GetAndroidJNIEnv"/>
        public static nint JniEnvironment => SDL.GetAndroidJNIEnv();

        /// <inheritdoc cref="CSDL.Internal.Docs.System.GetAndroidSDKVersion"/>
        public static int SdkVersion => SDL.GetAndroidSDKVersion();

        /// <inheritdoc cref="CSDL.Internal.Docs.System.GetAndroidInternalStoragePath"/>
        public static string? InternalStoragePath => SDL.GetAndroidInternalStoragePath().ToUtf8StringOrLog(nameof(SDL.GetAndroidInternalStoragePath));

        /// <inheritdoc cref="CSDL.Internal.Docs.System.GetAndroidExternalStoragePath"/>
        public static string? ExternalStoragePath => SDL.GetAndroidExternalStoragePath().ToUtf8StringOrLog(nameof(SDL.GetAndroidExternalStoragePath));

        /// <inheritdoc cref="CSDL.Internal.Docs.System.GetAndroidCachePath"/>
        public static string? CachePath => SDL.GetAndroidCachePath().ToUtf8StringOrLog(nameof(SDL.GetAndroidCachePath));

        /// <inheritdoc cref="CSDL.Internal.Docs.System.GetAndroidExternalStorageState"/>
        public static AndroidStorageState ExternalStorageState => (AndroidStorageState)SDL.GetAndroidExternalStorageState();

        /// <inheritdoc cref="CSDL.Internal.Docs.System.RequestAndroidPermission"/>
        /// <param name="permission">the Android permission name, e.g. <c>android.permission.RECORD_AUDIO</c>.</param>
        /// <param name="callback">invoked - possibly much later, from the app's main thread - with the user's answer.</param>
        /// <param name="userdata">passed through to the callback.</param>
        /// <remarks>
        ///     The callback is rooted until it fires. Do not block waiting for it: the answer only
        ///     arrives while the event loop keeps running.
        /// </remarks>
        public static bool RequestPermission(string permission, RequestAndroidPermissionCallback callback, object? userdata = null) {
            Error.ThrowIfNullOrWhiteSpace(permission, nameof(permission), nameof(SDL.RequestAndroidPermission));
            ArgumentNullException.ThrowIfNull(callback);

            string id = $"AndroidPermission:{permission}:{Guid.NewGuid()}";

            RequestAndroidPermissionCallback wrapper = (data, name, granted) => {
                try {
                    callback(data, name, granted);
                }
                finally {
                    CallbackRegistry.Unregister<RequestAndroidPermissionCallback, SDL_RequestAndroidPermissionCallbackNative>(id);
                }
            };

            SDL_RequestAndroidPermissionCallbackNative native = RequestAndroidPermissionCallbackWrapper.Create(wrapper);
            (IntPtr functionPtr, IntPtr userdataPtr) cb = CallbackRegistry.Register(id, wrapper, native, userdata);

            CBool ok = SDL.RequestAndroidPermission(permission, native, cb.userdataPtr);
            if (!ok) {
                CallbackRegistry.Unregister<RequestAndroidPermissionCallback, SDL_RequestAndroidPermissionCallbackNative>(id);
            }
            return ok.LogIfFalse(nameof(SDL.RequestAndroidPermission));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.System.SendAndroidBackButton"/>
        public static void SendBackButton() {
            SDL.SendAndroidBackButton();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.System.SendAndroidMessage"/>
        public static bool SendMessage(uint command, int param) {
            return SDL.SendAndroidMessage(command, param).LogIfFalse(nameof(SDL.SendAndroidMessage));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.System.ShowAndroidToast"/>
        /// <param name="message">the text to show.</param>
        /// <param name="durationSeconds">0 for a short toast, 1 for a long one.</param>
        /// <param name="gravity">the Android gravity constant to position the toast with, or -1 for the default.</param>
        /// <param name="xOffset">horizontal offset, ignored when <paramref name="gravity"/> is -1.</param>
        /// <param name="yOffset">vertical offset, ignored when <paramref name="gravity"/> is -1.</param>
        public static bool ShowToast(string message, int durationSeconds = 0, int gravity = -1, int xOffset = 0, int yOffset = 0) {
            Error.ThrowIfNullOrWhiteSpace(message, nameof(message), nameof(SDL.ShowAndroidToast));
            return SDL.ShowAndroidToast(message, durationSeconds, gravity, xOffset, yOffset).LogIfFalse(nameof(SDL.ShowAndroidToast));
        }
    }
}
