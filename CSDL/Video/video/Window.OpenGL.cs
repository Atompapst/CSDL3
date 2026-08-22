// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Extensions;

namespace CSDL.Video {
    public sealed partial class Window {
        /// <inheritdoc cref="CSDL.Internal.Docs.Video.GL_CreateContext"/>
        public GLContext CreateOpenGLContext() {
            NativePtr<CSDL.Opaque.SdlGLContext> context = SDL.GL_CreateContext(Handle).ThrowIfInvalid();
            return new GLContext(context, true);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.GL_SwapWindow"/>
        public bool SwapOpenGL() {
            return SDL.GL_SwapWindow(Handle).LogIfFalse();
        }
    }
}
