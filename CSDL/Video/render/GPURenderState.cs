// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Extensions;
using CSDL.GPU;

namespace CSDL.Video {
    public class GPURenderState : NativeHandle<CSDL.Opaque.SdlGPURenderState> {
        /// <inheritdoc cref="CSDL.Internal.Docs.Render.CreateGPURenderState"/>
        public GPURenderState(Renderer renderer, GPUShader fragmentShader, GPUTextureSamplerBinding[]? samplerBindings = null, GPUTexture[]? storageTextures = null, GPUBuffer[]? storageBuffers = null) {
            ArgumentNullException.ThrowIfNull(renderer);
            ArgumentNullException.ThrowIfNull(fragmentShader);

            samplerBindings ??= Array.Empty<GPUTextureSamplerBinding>();
            IntPtr[] textures = (storageTextures ?? Array.Empty<GPUTexture>()).GetRaw();
            IntPtr[] buffers = (storageBuffers ?? Array.Empty<GPUBuffer>()).GetRaw();

            unsafe {
                fixed (GPUTextureSamplerBinding* samplerPtr = samplerBindings) {
                    fixed (IntPtr* texturesPtr = textures) {
                        fixed (IntPtr* buffersPtr = buffers) {
                            GPURenderStateCreateInfo createInfo = new GPURenderStateCreateInfo {
                                FragmentShader = fragmentShader.NativePointer,
                                NumSamplerBindings = samplerBindings.Length,
                                SamplerBindings = (nint)samplerPtr,
                                NumStorageTextures = textures.Length,
                                StorageTextures = (nint)texturesPtr,
                                NumStorageBuffers = buffers.Length,
                                StorageBuffers = (nint)buffersPtr,
                                Props = default,
                            };
                            Handle = SDL.CreateGPURenderState(renderer.Handle, in createInfo).ThrowIfInvalid();
                        }
                    }
                }
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.SetGPURenderStateFragmentUniforms"/>
        public bool SetFragmentUniforms(uint slotIndex, byte[] data) {
            if (data == null || data.Length == 0) return true;
            unsafe {
                fixed (byte* b = data) {
                    return SDL.SetGPURenderStateFragmentUniforms(Handle, slotIndex, (IntPtr)b, (uint)data.Length).LogIfFalse();
                }
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.SetGPURenderStateSamplerBindings"/>
        public bool SetSamplerBindings(GPUTextureSamplerBinding[] samplerBindings) {
            if (samplerBindings == null || samplerBindings.Length == 0) return true;
            return SDL.SetGPURenderStateSamplerBindings(Handle, samplerBindings.Length, samplerBindings).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.SetGPURenderStateStorageBuffers"/>
        public bool SetStorageBuffers(GPUBuffer[] buffers) {
            if (buffers == null || buffers.Length == 0) return true;
            bool result = false;
            unsafe {
                buffers.WithPointers((ptr, count) => {
                    result = SDL.SetGPURenderStateStorageBuffers(Handle, (int)count, ptr).LogIfFalse();
                });
            }
            return result;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.SetGPURenderStateStorageTextures"/>
        public bool SetStorageTextures(GPUTexture[] textures) {
            if (textures == null || textures.Length == 0) return true;
            bool result = false;
            unsafe {
                textures.WithPointers((ptr, count) => {
                    result = SDL.SetGPURenderStateStorageTextures(Handle, (int)count, ptr).LogIfFalse();
                });
            }
            return result;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.DestroyGPURenderState"/>
        protected override void DisposeResource() {
            SDL.DestroyGPURenderState(Handle);
        }
    }
}
