// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Runtime.InteropServices;
using CSDL.Extensions;

namespace CSDL.Video {
    /// <summary>
    /// EGL-specific functions, for platforms that use EGL to create OpenGL/OpenGL ES contexts.
    /// </summary>
    public static class EGL {
        private const string PlatformCallbackId = "SDL.EGL.PlatformAttribCallback";
        private const string SurfaceCallbackId = "SDL.EGL.SurfaceAttribCallback";
        private const string ContextCallbackId = "SDL.EGL.ContextAttribCallback";

        private static IntPtr _userdataPtr;

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.EGL_GetCurrentConfig"/>
        public static IntPtr CurrentConfig => SDL.EGL_GetCurrentConfig().LogIfInvalid().Ptr;

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.EGL_GetCurrentDisplay"/>
        public static IntPtr CurrentDisplay => SDL.EGL_GetCurrentDisplay().LogIfInvalid().Ptr;

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.EGL_GetWindowSurface"/>
        public static IntPtr GetWindowSurface(Window window) {
            return SDL.EGL_GetWindowSurface(window.Handle).LogIfInvalid().Ptr;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.EGL_GetProcAddress"/>
        public static IntPtr? GetProcAddress(string proc) {
            ArgumentException.ThrowIfNullOrWhiteSpace(proc);
            IntPtr address = SDL.EGL_GetProcAddress(proc);
            return address == IntPtr.Zero ? null : address;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.EGL_SetAttributeCallbacks"/>
        public static void SetAttributeCallbacks(
            EGLAttribArrayCallback? platformAttribCallback,
            EGLIntArrayCallback? surfaceAttribCallback,
            EGLIntArrayCallback? contextAttribCallback,
            object? userData = null) {
            CallbackRegistry.Unregister<EGLAttribArrayCallback, SDL_EGLAttribArrayCallbackNative>(PlatformCallbackId);
            CallbackRegistry.Unregister<EGLIntArrayCallback, SDL_EGLIntArrayCallbackNative>(SurfaceCallbackId);
            CallbackRegistry.Unregister<EGLIntArrayCallback, SDL_EGLIntArrayCallbackNative>(ContextCallbackId);

            if (_userdataPtr != IntPtr.Zero) {
                GCHandle.FromIntPtr(_userdataPtr).Free();
                _userdataPtr = IntPtr.Zero;
            }
            if (userData != null) {
                _userdataPtr = GCHandle.ToIntPtr(GCHandle.Alloc(userData));
            }

            SDL_EGLAttribArrayCallbackNative? platformNative = null;
            if (platformAttribCallback != null) {
                platformNative = EGLAttribArrayCallbackWrapper.Create(platformAttribCallback);
                CallbackRegistry.Register(PlatformCallbackId, platformAttribCallback, platformNative);
            }

            SDL_EGLIntArrayCallbackNative? surfaceNative = null;
            if (surfaceAttribCallback != null) {
                surfaceNative = EGLIntArrayCallbackWrapper.Create(surfaceAttribCallback);
                CallbackRegistry.Register(SurfaceCallbackId, surfaceAttribCallback, surfaceNative);
            }

            SDL_EGLIntArrayCallbackNative? contextNative = null;
            if (contextAttribCallback != null) {
                contextNative = EGLIntArrayCallbackWrapper.Create(contextAttribCallback);
                CallbackRegistry.Register(ContextCallbackId, contextAttribCallback, contextNative);
            }

            SDL.EGL_SetAttributeCallbacks(platformNative!, surfaceNative!, contextNative!, _userdataPtr);
        }
    }
}
