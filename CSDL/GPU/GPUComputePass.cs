// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Extensions;
using System;
namespace CSDL.GPU {
    public class GPUComputePass : NativeHandle<Opaque.SdlGPUComputePass>, IGPUPass {
        private GPUCommandBuffer? _commandBuffer;

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.BeginGPUComputePass"/>
        public GPUComputePass(GPUCommandBuffer gpuCommandBuffer, GPUStorageTextureReadWriteBinding[]? storageTextureBindings = null, GPUStorageBufferReadWriteBinding[]? storageBufferBindings = null) {
            ArgumentNullException.ThrowIfNull(gpuCommandBuffer);
            storageTextureBindings ??= Array.Empty<GPUStorageTextureReadWriteBinding>();
            storageBufferBindings ??= Array.Empty<GPUStorageBufferReadWriteBinding>();
            _commandBuffer = gpuCommandBuffer;
            gpuCommandBuffer.BeginPass(this);

            try {
                unsafe {
                    fixed (GPUStorageTextureReadWriteBinding* ptr = storageTextureBindings) {
                        fixed (GPUStorageBufferReadWriteBinding* ptr2 = storageBufferBindings) {
                            Handle = SDL.BeginGPUComputePass(gpuCommandBuffer.Handle,
                                ptr, (uint)storageTextureBindings.Length,
                                ptr2, (uint)storageBufferBindings.Length).ThrowIfInvalid();
                        }
                    }
                }
            } catch {
                _commandBuffer = null;
                gpuCommandBuffer.EndPass(this);
                throw;
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.BindGPUComputePipeline"/>
        public void BindComputePipeline(GPUComputePipeline pipeline) {
            ArgumentNullException.ThrowIfNull(pipeline);
            SDL.BindGPUComputePipeline(Handle, pipeline.Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.BindGPUComputeSamplers"/>
        public void BindComputeSamplers(uint firstSlot, GPUTextureSamplerBinding[] bindings) {
            if (bindings == null || bindings.Length == 0) return;
            unsafe {
                fixed (GPUTextureSamplerBinding* ptr = bindings) {
                    SDL.BindGPUComputeSamplers(Handle, firstSlot, ptr, (uint)bindings.Length);
                }
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.BindGPUComputeSamplers"/>
        public void BindComputeSamplers(uint firstSlot, GPUTexture[] textures, GPUSampler[] samplers) {
            if (textures == null || samplers == null || textures.Length == 0) return;
            if (textures.Length != samplers.Length) {
                throw new ArgumentException("Textures length must match samplers length.", nameof(textures));
            }
            unsafe {
                GPUTextureSamplerBinding[] bindings = textures.Combine(samplers);
                fixed (GPUTextureSamplerBinding* ptr = bindings) {
                    SDL.BindGPUComputeSamplers(Handle, firstSlot, ptr, (uint)bindings.Length);
                }
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.BindGPUComputeStorageTextures"/>
        public void BindComputeStorageTextures(uint firstSlot, GPUTexture[] textures) {
            if (textures == null || textures.Length == 0) return;
            unsafe {
                textures.WithPointers((ptr, count) => {
                    SDL.BindGPUComputeStorageTextures(Handle, firstSlot, ptr, count);
                });
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.BindGPUComputeStorageBuffers"/>
        public void BindComputeStorageBuffers(uint firstSlot, GPUBuffer[] buffers) {
            if (buffers == null || buffers.Length == 0) return;

            unsafe {
                buffers.WithPointers((ptr, count) => {
                    SDL.BindGPUComputeStorageBuffers(Handle, firstSlot, ptr, count);
                });
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.DispatchGPUCompute"/>
        public void Dispatch(uint groupcountX, uint groupcountY = 1, uint groupcountZ = 1) {
            SDL.DispatchGPUCompute(Handle, groupcountX, groupcountY, groupcountZ);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.DispatchGPUComputeIndirect"/>
        public void DispatchIndirect(GPUBuffer gpuBuffer, uint offset) {
            ArgumentNullException.ThrowIfNull(gpuBuffer);
            SDL.DispatchGPUComputeIndirect(Handle, gpuBuffer.Handle, offset);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.EndGPUComputePass"/>
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
            SDL.EndGPUComputePass(Handle);
            GPUCommandBuffer? commandBuffer = _commandBuffer;
            _commandBuffer = null;
            commandBuffer?.EndPass(this);
        }
    }
}
