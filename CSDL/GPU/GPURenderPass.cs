// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Extensions;
using System;
using CSDL.Video;
namespace CSDL.GPU {
    public class GPURenderPass : NativeHandle<Opaque.SdlGPURenderPass>, IGPUPass {
        private GPUCommandBuffer? _commandBuffer;

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.BeginGPURenderPass"/>
        public GPURenderPass(GPUCommandBuffer gpuCommandBuffer, GPUColorTargetInfo[]? colorTargetInfos = null, GPUDepthStencilTargetInfo? depthStencilTargetInfo = null) {
            ArgumentNullException.ThrowIfNull(gpuCommandBuffer);
            colorTargetInfos ??= Array.Empty<GPUColorTargetInfo>();
            _commandBuffer = gpuCommandBuffer;
            gpuCommandBuffer.BeginPass(this);

            try {
                unsafe {
                    fixed (GPUColorTargetInfo* colorTargetInfosPtr = colorTargetInfos) {
                        if (depthStencilTargetInfo.HasValue) {
                            GPUDepthStencilTargetInfo dsInfo = depthStencilTargetInfo.Value;
                            Handle = SDL.BeginGPURenderPass(gpuCommandBuffer.Handle, colorTargetInfosPtr, (uint)colorTargetInfos.Length, &dsInfo).ThrowIfInvalid();
                        } else {
                            Handle = SDL.BeginGPURenderPass(gpuCommandBuffer.Handle, colorTargetInfosPtr, (uint)colorTargetInfos.Length, null).ThrowIfInvalid();
                        }
                    }
                }
            } catch {
                _commandBuffer = null;
                gpuCommandBuffer.EndPass(this);
                throw;
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.BindGPUGraphicsPipeline"/>
        public void BindGraphicsPipeline(GPUGraphicsPipeline pipeline) {
            ArgumentNullException.ThrowIfNull(pipeline);
            SDL.BindGPUGraphicsPipeline(Handle, pipeline.Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.SetGPUViewport"/>
        public void SetViewport(in GPUViewport viewport) {
            SDL.SetGPUViewport(Handle, in viewport);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.SetGPUScissor"/>
        public void SetScissor(Rect scissor) {
            SDL.SetGPUScissor(Handle, in scissor);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.SetGPUBlendConstants"/>
        public void SetBlendConstants(FColor blendConstants) {
            SDL.SetGPUBlendConstants(Handle, blendConstants);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.SetGPUStencilReference"/>
        public void SetStencilReference(byte reference) {
            SDL.SetGPUStencilReference(Handle, reference);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.BindGPUVertexBuffers"/>
        public void BindVertexBuffers(uint firstSlot, GPUBufferBinding[] bindings) {
            if (bindings == null || bindings.Length == 0) return;
            unsafe {
                fixed (GPUBufferBinding* ptr = bindings) {
                    SDL.BindGPUVertexBuffers(Handle, firstSlot, ptr, (uint)bindings.Length);
                }
            }

        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.BindGPUVertexBuffers"/>
        public void BindVertexBuffers(uint firstSlot, GPUBuffer[] buffers, uint[]? offsets = null) {
            if (buffers == null || buffers.Length == 0) return;
            if (offsets != null && offsets.Length != buffers.Length) {
                throw new ArgumentException("Offsets length must match buffers length.", nameof(offsets));
            }

            GPUBufferBinding[] bindings = new GPUBufferBinding[buffers.Length];
            for (int i = 0; i < buffers.Length; i++) {
                bindings[i] = new GPUBufferBinding {
                    Buffer = buffers[i].Handle,
                    Offset = offsets != null ? offsets[i] : 0u,
                };
            }
            unsafe {
                fixed (GPUBufferBinding* bindingsPtr = bindings) {
                    SDL.BindGPUVertexBuffers(Handle, firstSlot, bindingsPtr, (uint)bindings.Length);
                }
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.BindGPUIndexBuffer"/>
        public void BindIndexBuffer(GPUBuffer gpuBuffer, uint offset, GPUIndexElementSize indexElementSize) {
            ArgumentNullException.ThrowIfNull(gpuBuffer);
            GPUBufferBinding binding = new GPUBufferBinding {
                Buffer = gpuBuffer.Handle,
                Offset = offset,
            };
            SDL.BindGPUIndexBuffer(Handle, in binding, indexElementSize);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.BindGPUVertexSamplers"/>
        public void BindVertexSamplers(uint firstSlot, GPUTextureSamplerBinding[] bindings) {
            if (bindings == null || bindings.Length == 0) return;
            unsafe {
                fixed (GPUTextureSamplerBinding* bindingsPtr = bindings) {
                    SDL.BindGPUVertexSamplers(Handle, firstSlot, bindingsPtr, (uint)bindings.Length);
                }
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.BindGPUVertexSamplers"/>
        public void BindVertexSamplers(uint firstSlot, GPUTexture[] textures, GPUSampler[] samplers) {
            if (textures == null || samplers == null || textures.Length == 0) return;
            if (textures.Length != samplers.Length) {
                throw new ArgumentException("Textures length must match samplers length.", nameof(textures));
            }

            GPUTextureSamplerBinding[] bindings = new GPUTextureSamplerBinding[textures.Length];
            for (int i = 0; i < textures.Length; i++) {
                bindings[i] = new GPUTextureSamplerBinding {
                    Texture = textures[i].Handle,
                    Sampler = samplers[i].Handle,
                };
            }

            unsafe {
                fixed (GPUTextureSamplerBinding* bindingsPtr = bindings) {
                    SDL.BindGPUVertexSamplers(Handle, firstSlot, bindingsPtr, (uint)bindings.Length);
                }
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.BindGPUVertexStorageTextures"/>
        public void BindVertexStorageTextures(uint firstSlot, GPUTexture[] textures) {
            if (textures == null || textures.Length == 0) return;
            unsafe {
                textures.WithPointers((ptr, count) => {
                    SDL.BindGPUVertexStorageTextures(Handle, firstSlot, ptr, count);
                });
            }

        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.BindGPUVertexStorageBuffers"/>
        public void BindVertexStorageBuffers(uint firstSlot, GPUBuffer[] buffers) {
            if (buffers == null || buffers.Length == 0) return;
            unsafe {
                buffers.WithPointers((ptr, count) => {
                    SDL.BindGPUVertexStorageBuffers(Handle, firstSlot, ptr, count);
                });
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.BindGPUFragmentSamplers"/>
        public void BindFragmentSamplers(uint firstSlot, GPUTextureSamplerBinding[] bindings) {
            if (bindings == null || bindings.Length == 0) return;
            unsafe {
                fixed (GPUTextureSamplerBinding* bindingsPtr = bindings) {
                    SDL.BindGPUFragmentSamplers(Handle, firstSlot, bindingsPtr, (uint)bindings.Length);
                }
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.BindGPUFragmentSamplers"/>
        public void BindFragmentSamplers(uint firstSlot, GPUTexture[] textures, GPUSampler[] samplers) {
            if (textures == null || samplers == null || textures.Length == 0) return;
            if (textures.Length != samplers.Length) {
                throw new ArgumentException("Textures length must match samplers length.", nameof(textures));
            }
            GPUTextureSamplerBinding[] bindings = textures.Combine(samplers);

            unsafe {
                fixed (GPUTextureSamplerBinding* bindingsPtr = bindings) {
                    SDL.BindGPUFragmentSamplers(Handle, firstSlot, bindingsPtr, (uint)bindings.Length);
                }
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.BindGPUFragmentStorageTextures"/>
        public void BindFragmentStorageTextures(uint firstSlot, GPUTexture[] textures) {
            if (textures == null || textures.Length == 0) return;
            unsafe {
                textures.WithPointers((ptr, count) => {
                    SDL.BindGPUFragmentStorageTextures(Handle, firstSlot, ptr, count);
                });
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.BindGPUFragmentStorageBuffers"/>
        public void BindFragmentStorageBuffers(uint firstSlot, GPUBuffer[] buffers) {
            if (buffers == null || buffers.Length == 0) return;
            unsafe {
                buffers.WithPointers((ptr, count) => {
                    SDL.BindGPUFragmentStorageBuffers(Handle, firstSlot, ptr, count);
                });
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.DrawGPUIndexedPrimitives"/>
        public void DrawIndexed(uint numIndices, uint numInstances = 1, uint firstIndex = 0, int vertexOffset = 0, uint firstInstance = 0) {
            SDL.DrawGPUIndexedPrimitives(Handle, numIndices, numInstances, firstIndex, vertexOffset, firstInstance);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.DrawGPUPrimitives"/>
        public void Draw(uint numVertices, uint numInstances = 1, uint firstVertex = 0, uint firstInstance = 0) {
            SDL.DrawGPUPrimitives(Handle, numVertices, numInstances, firstVertex, firstInstance);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.DrawGPUPrimitivesIndirect"/>
        public void DrawIndirect(GPUBuffer gpuBuffer, uint offset, uint drawCount) {
            ArgumentNullException.ThrowIfNull(gpuBuffer);
            SDL.DrawGPUPrimitivesIndirect(Handle, gpuBuffer.Handle, offset, drawCount);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.DrawGPUIndexedPrimitivesIndirect"/>
        public void DrawIndexedIndirect(GPUBuffer gpuBuffer, uint offset, uint drawCount) {
            ArgumentNullException.ThrowIfNull(gpuBuffer);
            SDL.DrawGPUIndexedPrimitivesIndirect(Handle, gpuBuffer.Handle, offset, drawCount);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.EndGPURenderPass"/>
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
            SDL.EndGPURenderPass(Handle);
            GPUCommandBuffer? commandBuffer = _commandBuffer;
            _commandBuffer = null;
            commandBuffer?.EndPass(this);
        }
    }
}
