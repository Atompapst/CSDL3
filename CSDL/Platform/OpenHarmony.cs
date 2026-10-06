// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Extensions;

namespace CSDL {
    /// <summary>
    ///     OpenHarmony/HarmonyOS-only entry points. On every other platform these fail (or return
    ///     zero/empty) and set an SDL error.
    /// </summary>
    public static class OpenHarmony {
        /// <inheritdoc cref="CSDL.Internal.Docs.System.GetOpenHarmonySDKVersion"/>
        public static int SdkVersion => SDL.GetOpenHarmonySDKVersion();

        /// <inheritdoc cref="CSDL.Internal.Docs.System.GetOpenHarmonyInternalStoragePath"/>
        public static string? InternalStoragePath => SDL.GetOpenHarmonyInternalStoragePath().ToUtf8StringOrLog(nameof(SDL.GetOpenHarmonyInternalStoragePath));

        /// <inheritdoc cref="CSDL.Internal.Docs.System.RequestOpenHarmonyPermission"/>
        /// <param name="permission">the OpenHarmony permission name, e.g. <c>ohos.permission.MICROPHONE</c>.</param>
        /// <param name="callback">invoked - possibly much later, from the app's main thread - with the user's answer.</param>
        /// <param name="userdata">passed through to the callback.</param>
        /// <remarks>
        ///     The callback is rooted until it fires. Do not block waiting for it: the answer only
        ///     arrives while the event loop keeps running.
        /// </remarks>
        public static bool RequestPermission(string permission, RequestOpenHarmonyPermissionCallback callback, object? userdata = null) {
            Error.ThrowIfNullOrWhiteSpace(permission, nameof(permission), nameof(SDL.RequestOpenHarmonyPermission));
            ArgumentNullException.ThrowIfNull(callback);

            string id = $"OpenHarmonyPermission:{permission}:{Guid.NewGuid()}";

            RequestOpenHarmonyPermissionCallback wrapper = (data, name, granted) => {
                try {
                    callback(data, name, granted);
                }
                finally {
                    CallbackRegistry.Unregister<RequestOpenHarmonyPermissionCallback, SDL_RequestOpenHarmonyPermissionCallbackNative>(id);
                }
            };

            SDL_RequestOpenHarmonyPermissionCallbackNative native = RequestOpenHarmonyPermissionCallbackWrapper.Create(wrapper);
            (IntPtr functionPtr, IntPtr userdataPtr) cb = CallbackRegistry.Register(id, wrapper, native, userdata);

            CBool ok = SDL.RequestOpenHarmonyPermission(permission, native, cb.userdataPtr);
            if (!ok) {
                CallbackRegistry.Unregister<RequestOpenHarmonyPermissionCallback, SDL_RequestOpenHarmonyPermissionCallbackNative>(id);
            }
            return ok.LogIfFalse(nameof(SDL.RequestOpenHarmonyPermission));
        }
    }
}
