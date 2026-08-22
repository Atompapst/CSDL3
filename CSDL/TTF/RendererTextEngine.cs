// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Extensions;
using CSDL.Video;

namespace CSDL.TTF {
    /// <summary>
    /// A text engine that lays out <see cref="TextObject"/> instances for drawing on an SDL
    /// <see cref="Renderer"/>.
    /// </summary>
    public sealed class RendererTextEngine : TextEngine {
        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.CreateRendererTextEngine"/>
        public RendererTextEngine(Renderer renderer) {
            Handle = SDL.CreateRendererTextEngine(renderer.Handle).ThrowIfInvalid();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.CreateRendererTextEngineWithProperties"/>
        public RendererTextEngine(RendererTextEngineCreateProperties properties) {
            Handle = SDL.CreateRendererTextEngineWithProperties(properties.Handle).ThrowIfInvalid();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.DestroyRendererTextEngine"/>
        protected override void DisposeResource() {
            SDL.DestroyRendererTextEngine(Handle);
        }
    }
}
