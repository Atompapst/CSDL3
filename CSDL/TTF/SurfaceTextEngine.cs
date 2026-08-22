// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Extensions;

namespace CSDL.TTF {
    /// <summary>
    /// A text engine that lays out <see cref="TextObject"/> instances for drawing onto SDL
    /// surfaces.
    /// </summary>
    public sealed class SurfaceTextEngine : TextEngine {
        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.CreateSurfaceTextEngine"/>
        public SurfaceTextEngine() {
            Handle = SDL.CreateSurfaceTextEngine().ThrowIfInvalid();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.DestroySurfaceTextEngine"/>
        protected override void DisposeResource() {
            SDL.DestroySurfaceTextEngine(Handle);
        }
    }
}
