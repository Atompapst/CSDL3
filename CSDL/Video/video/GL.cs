// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Extensions;

namespace CSDL.Video {
    /// <summary>
    /// Library-level OpenGL functions: loading the GL library, querying extensions and
    /// attributes, and controlling the swap interval. For window/context-specific
    /// operations, see <see cref="Window"/> and <see cref="GLContext"/>.
    /// </summary>
    public static class GL {
        static GL() {
            Init.InitSubSystem(InitFlags.Video);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.GL_GetCurrentContext"/>
        public static GLContext? CurrentContext {
            get {
                NativePtr<CSDL.Opaque.SdlGLContext> context = SDL.GL_GetCurrentContext().LogIfInvalid();
                return context.IsNull ? null : new GLContext(context, false);
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.GL_GetCurrentWindow"/>
        public static Window? CurrentWindow {
            get {
                NativePtr<CSDL.Opaque.SdlWindow> window = SDL.GL_GetCurrentWindow().LogIfInvalid();
                return window.IsNull ? null : new Window(window);
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.GL_LoadLibrary"/>
        public static bool LoadLibrary(string? path = null) {
            return SDL.GL_LoadLibrary(path).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.GL_UnloadLibrary"/>
        public static void UnloadLibrary() {
            SDL.GL_UnloadLibrary();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.GL_GetProcAddress"/>
        public static IntPtr? GetProcAddress(string proc) {
            ArgumentException.ThrowIfNullOrWhiteSpace(proc);
            IntPtr address = SDL.GL_GetProcAddress(proc);
            return address == IntPtr.Zero ? null : address;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.GL_ExtensionSupported"/>
        public static bool ExtensionSupported(string extension) {
            return SDL.GL_ExtensionSupported(extension);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.GL_ResetAttributes"/>
        public static void ResetAttributes() {
            SDL.GL_ResetAttributes();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.GL_SetAttribute"/>
        public static bool SetAttribute(GLAttr attr, int value) {
            return SDL.GL_SetAttribute(attr, value).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.GL_GetAttribute"/>
        public static bool TryGetAttribute(GLAttr attr, out int value) {
            return SDL.GL_GetAttribute(attr, out value).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.GL_SetSwapInterval"/>
        public static int SwapInterval {
            get {
                SDL.GL_GetSwapInterval(out int interval).LogIfFalse();
                return interval;
            }
            set => SDL.GL_SetSwapInterval(value).LogIfFalse();
        }
    }
}
