// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Extensions;
namespace CSDL.GPU {
    public class GPUCopyPass : NativeHandle<Opaque.SdlGPUCopyPass>, IGPUPass {
        private GPUCommandBuffer? _commandBuffer;

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.BeginGPUCopyPass"/>
        public GPUCopyPass(GPUCommandBuffer buffer) {
            ArgumentNullException.ThrowIfNull(buffer);
            _commandBuffer = buffer;
            buffer.BeginPass(this);
            try {
                Handle = SDL.BeginGPUCopyPass(buffer.Handle).ThrowIfInvalid();
            } catch {
                _commandBuffer = null;
                buffer.EndPass(this);
                throw;
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.DownloadFromGPUBuffer"/>
        public void DownloadFromGPUBuffer(in GPUBufferRegion source, in GPUTransferBufferLocation destination) {
            SDL.DownloadFromGPUBuffer(Handle, source, destination);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.EndGPUCopyPass"/>
        protected override void DisposeResource() {
            EndPass();
        }

        void IGPUPass.EndFromCommandBuffer() {
            EndPass();
            Invalidate();
        }

        void IGPUPass.InvalidateFromCommandBuffer() {
            _commandBuffer = null;
            Invalidate();
        }

        private void EndPass() {
            SDL.EndGPUCopyPass(Handle);
            GPUCommandBuffer? commandBuffer = _commandBuffer;
            _commandBuffer = null;
            commandBuffer?.EndPass(this);
        }
    }
}
