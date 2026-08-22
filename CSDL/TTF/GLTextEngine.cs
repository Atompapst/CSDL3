// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Extensions;

namespace CSDL.TTF {
    /// <summary>
    /// A text engine that lays out <see cref="TextObject"/> instances for drawing with OpenGL.
    /// </summary>
    public sealed class GLTextEngine : TextEngine {
        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.CreateGLTextEngine"/>
        public GLTextEngine() {
            Handle = SDL.CreateGLTextEngine().ThrowIfInvalid();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.CreateGLTextEngineWithProperties"/>
        public GLTextEngine(GLTextEngineCreateProperties properties) {
            Handle = SDL.CreateGLTextEngineWithProperties(properties.Handle).ThrowIfInvalid();
        }

        /// <summary>
        /// Gets or sets the winding order of the vertices returned by <c>TTF_GetGLTextDrawData</c>.
        /// </summary>
        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.GetGLTextEngineWinding"/>
        public GLTextEngineWinding Winding {
            get => SDL.GetGLTextEngineWinding(Handle);
            set => SDL.SetGLTextEngineWinding(Handle, value);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.DestroyGLTextEngine"/>
        protected override void DisposeResource() {
            SDL.DestroyGLTextEngine(Handle);
        }
    }
}
