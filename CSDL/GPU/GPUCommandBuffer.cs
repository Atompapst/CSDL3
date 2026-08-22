// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Extensions;

namespace CSDL.GPU {
    internal interface IGPUPass {
        void EndFromCommandBuffer();
        void InvalidateFromCommandBuffer();
    }

    public class GPUCommandBuffer : NativeHandle<Opaque.SdlGPUCommandBuffer> {
        private readonly GPUDevice _gpuDevice;
        private IGPUPass? _activePass;
        private bool _submitted;
        private bool _hasSwapchainTexture;

        internal GPUDevice Device => _gpuDevice;

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.AcquireGPUCommandBuffer"/>
        public GPUCommandBuffer(GPUDevice gpuDevice) {
            ArgumentNullException.ThrowIfNull(gpuDevice);
            _gpuDevice = gpuDevice;
            Handle = SDL.AcquireGPUCommandBuffer(gpuDevice.Handle).ThrowIfInvalid();
            gpuDevice.TrackChild(Invalidation);
        }

        /// <summary>
        /// Begins a render pass scoped to this command buffer. Dispose (or use in a <c>using</c> block)
        /// to end the pass via <see cref="CSDL.Internal.Docs.GPU.EndGPURenderPass"/>.
        /// </summary>
        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.BeginGPURenderPass"/>
        public GPURenderPass BeginRenderPass(GPUColorTargetInfo[]? colorTargetInfos = null, GPUDepthStencilTargetInfo? depthStencilTargetInfo = null) {
            EnsureRecording();
            return new GPURenderPass(this, colorTargetInfos, depthStencilTargetInfo);
        }

        /// <summary>
        /// Begins a compute pass scoped to this command buffer. Dispose (or use in a <c>using</c> block)
        /// to end the pass via <see cref="CSDL.Internal.Docs.GPU.EndGPUComputePass"/>.
        /// </summary>
        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.BeginGPUComputePass"/>
        public GPUComputePass BeginComputePass(GPUStorageTextureReadWriteBinding[]? storageTextureBindings = null, GPUStorageBufferReadWriteBinding[]? storageBufferBindings = null) {
            EnsureRecording();
            return new GPUComputePass(this, storageTextureBindings, storageBufferBindings);
        }

        /// <summary>
        /// Begins a copy pass scoped to this command buffer. Dispose (or use in a <c>using</c> block)
        /// to end the pass via <see cref="CSDL.Internal.Docs.GPU.EndGPUCopyPass"/>.
        /// </summary>
        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.BeginGPUCopyPass"/>
        public GPUCopyPass BeginCopyPass() {
            EnsureRecording();
            return new GPUCopyPass(this);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.SubmitGPUCommandBufferAndAcquireFence"/>
        public GPUFence SubmitAndAcquireFence() {
            EnsureReadyToFinalize();
            try {
                NativePtr<Opaque.SdlGPUFence> fence = SDL.SubmitGPUCommandBufferAndAcquireFence(Handle).ThrowIfInvalid();
                return new GPUFence(_gpuDevice, fence);
            } finally {
                // SDL invalidates a command buffer after a submission attempt, including failures.
                _submitted = true;
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.SubmitGPUCommandBufferAndAcquireFence"/>
        public GPUFence SubmitAndAcquireFence(GPUDevice gpuDevice) {
            ArgumentNullException.ThrowIfNull(gpuDevice);
            if (!ReferenceEquals(gpuDevice, _gpuDevice)) {
                throw new ArgumentException("The fence must belong to the device that acquired this command buffer.", nameof(gpuDevice));
            }

            return SubmitAndAcquireFence();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.BlitGPUTexture"/>
        public void BlitGPUTexture(GPUBlitInfo info) {
            EnsureOutsidePass();
            SDL.BlitGPUTexture(Handle, info);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.GenerateMipmapsForGPUTexture"/>
        public void GenerateMipmaps(GPUTexture texture) {
            ArgumentNullException.ThrowIfNull(texture);
            EnsureOutsidePass();
            SDL.GenerateMipmapsForGPUTexture(Handle, texture.Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.InsertGPUDebugLabel"/>
        public void InsertDebugLabel(string text) {
            EnsureRecording();
            SDL.InsertGPUDebugLabel(Handle, text);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.PushGPUDebugGroup"/>
        public void PushDebugGroup(string name) {
            EnsureRecording();
            SDL.PushGPUDebugGroup(Handle, name);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.PopGPUDebugGroup"/>
        public void PopDebugGroup() {
            EnsureRecording();
            SDL.PopGPUDebugGroup(Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.SubmitGPUCommandBuffer"/>
        public bool Submit() {
            if (_submitted) return false;
            EnsureReadyToFinalize();
            try {
                return SDL.SubmitGPUCommandBuffer(Handle).LogIfFalse();
            } finally {
                _submitted = true;
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.CancelGPUCommandBuffer"/>
        public void Cancel() {
            if (_submitted) return;
            EnsureReadyToFinalize();
            if (_hasSwapchainTexture) {
                throw new InvalidOperationException("A command buffer that acquired a swapchain texture must be submitted, not canceled.");
            }

            try {
                SDL.CancelGPUCommandBuffer(Handle).LogIfFalse();
            } finally {
                _submitted = true;
            }
        }

        internal void BeginPass(IGPUPass pass) {
            EnsureRecording();
            if (_activePass != null) {
                throw new InvalidOperationException("A GPU command buffer can only have one active pass.");
            }

            _activePass = pass;
        }

        internal void EndPass(IGPUPass pass) {
            if (ReferenceEquals(_activePass, pass)) {
                _activePass = null;
            }
        }

        internal void MarkSwapchainTextureAcquired() {
            EnsureRecording();
            _hasSwapchainTexture = true;
        }

        internal void EnsureRecording() {
            if (_submitted) {
                throw new InvalidOperationException("The GPU command buffer has already been submitted or canceled.");
            }
        }

        internal void EnsureOutsidePass() {
            EnsureRecording();
            if (_activePass != null) {
                throw new InvalidOperationException("This command is only valid outside a GPU pass.");
            }
        }

        private void EnsureReadyToFinalize() {
            EnsureRecording();
            if (_activePass != null) {
                throw new InvalidOperationException("End the active GPU pass before submitting or canceling its command buffer.");
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.CancelGPUCommandBuffer"/>
        protected override void DisposeResource() {
            if (_activePass != null) {
                _activePass.EndFromCommandBuffer();
            }

            if (_submitted) return;
            try {
                if (_hasSwapchainTexture) {
                    SDL.SubmitGPUCommandBuffer(Handle).LogIfFalse();
                } else {
                    SDL.CancelGPUCommandBuffer(Handle).LogIfFalse();
                }
            } finally {
                _submitted = true;
            }
        }

        protected override void InvalidateResource() {
            _activePass?.InvalidateFromCommandBuffer();
            _activePass = null;
            _submitted = true;
        }
    }
}
