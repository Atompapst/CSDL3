// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Extensions;
using CSDL.Video;

namespace CSDL {
    /// <summary>
    ///     iOS/tvOS-only entry points, plus the application lifecycle hooks an app has to forward when
    ///     it drives the platform's event loop itself instead of letting SDL do it.
    /// </summary>
    public static class Apple {
        /// <inheritdoc cref="CSDL.Internal.Docs.System.SetiOSAnimationCallback"/>
        /// <param name="window">the window to tie the callback's lifetime to.</param>
        /// <param name="interval">how many frames to wait between calls (1 = every frame).</param>
        /// <param name="callback">the function to run each interval, or <see langword="null"/> to stop.</param>
        /// <param name="userdata">passed through to the callback.</param>
        /// <remarks>
        ///     Replaces whatever callback was installed before. The delegate stays rooted for as long as
        ///     it is installed.
        /// </remarks>
        public static bool SetAnimationCallback(Window window, int interval, iOSAnimationCallback? callback, object? userdata = null) {
            ArgumentNullException.ThrowIfNull(window);

            if (callback is null) {
                CBool cleared = SDL.SetiOSAnimationCallback(window.Handle, interval, null!, IntPtr.Zero);
                CallbackRegistry.UnregisterSingle<iOSAnimationCallback, SDL_iOSAnimationCallbackNative>();
                return cleared.LogIfFalse(nameof(SDL.SetiOSAnimationCallback));
            }

            SDL_iOSAnimationCallbackNative native = iOSAnimationCallbackWrapper.Create(callback);
            (IntPtr functionPtr, IntPtr userdataPtr) cb = CallbackRegistry.RegisterSingle(callback, native, userdata);

            CBool ok = SDL.SetiOSAnimationCallback(window.Handle, interval, native, cb.userdataPtr);
            if (!ok) {
                CallbackRegistry.UnregisterSingle<iOSAnimationCallback, SDL_iOSAnimationCallbackNative>();
            }
            return ok.LogIfFalse(nameof(SDL.SetiOSAnimationCallback));
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.System.SetiOSEventPump"/>
        public static void SetEventPump(bool enabled) {
            SDL.SetiOSEventPump(enabled);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.System.OnApplicationWillTerminate"/>
        public static void OnWillTerminate() {
            SDL.OnApplicationWillTerminate();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.System.OnApplicationDidReceiveMemoryWarning"/>
        public static void OnDidReceiveMemoryWarning() {
            SDL.OnApplicationDidReceiveMemoryWarning();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.System.OnApplicationWillEnterBackground"/>
        public static void OnWillEnterBackground() {
            SDL.OnApplicationWillEnterBackground();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.System.OnApplicationDidEnterBackground"/>
        public static void OnDidEnterBackground() {
            SDL.OnApplicationDidEnterBackground();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.System.OnApplicationWillEnterForeground"/>
        public static void OnWillEnterForeground() {
            SDL.OnApplicationWillEnterForeground();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.System.OnApplicationDidEnterForeground"/>
        public static void OnDidEnterForeground() {
            SDL.OnApplicationDidEnterForeground();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.System.OnApplicationDidChangeStatusBarOrientation"/>
        public static void OnDidChangeStatusBarOrientation() {
            SDL.OnApplicationDidChangeStatusBarOrientation();
        }
    }
}
