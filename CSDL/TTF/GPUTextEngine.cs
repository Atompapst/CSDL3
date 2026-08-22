// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Extensions;
using CSDL.GPU;

namespace CSDL.TTF {
    /// <summary>
    /// A text engine that lays out <see cref="TextObject"/> instances for drawing with the SDL GPU
    /// API.
    /// </summary>
    public sealed class GPUTextEngine : TextEngine {
        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.CreateGPUTextEngine"/>
        public GPUTextEngine(GPUDevice device) {
            Handle = SDL.CreateGPUTextEngine(device.Handle).ThrowIfInvalid();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.CreateGPUTextEngineWithProperties"/>
        public GPUTextEngine(GPUTextEngineCreateProperties properties) {
            Handle = SDL.CreateGPUTextEngineWithProperties(properties.Handle).ThrowIfInvalid();
        }

        /// <summary>
        /// Gets or sets the winding order of the vertices returned by <c>TTF_GetGPUTextDrawData</c>.
        /// </summary>
        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.GetGPUTextEngineWinding"/>
        public GPUTextEngineWinding Winding {
            get => SDL.GetGPUTextEngineWinding(Handle);
            set => SDL.SetGPUTextEngineWinding(Handle, value);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.DestroyGPUTextEngine"/>
        protected override void DisposeResource() {
            SDL.DestroyGPUTextEngine(Handle);
        }
    }
}
