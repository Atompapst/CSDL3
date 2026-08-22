// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

namespace CSDL.Video {
    public sealed class GLContext : NativeHandle<CSDL.Opaque.SdlGLContext> {
        internal GLContext(NativePtr<CSDL.Opaque.SdlGLContext> handle, bool ownsHandle) : base(handle, ownsHandle) {
            Handle = handle;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.GL_DestroyContext"/>
        protected override void DisposeResource() {
            if (!SDL.GL_DestroyContext(Handle)) {
                Error.LogError(nameof(DisposeResource));
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Video.GL_MakeCurrent"/>
        public bool MakeCurrent(Window window) {
            if (!SDL.GL_MakeCurrent(window.Handle, Handle)) {
                Error.LogError(nameof(MakeCurrent));
                return false;
            }

            return true;
        }
    }
}
