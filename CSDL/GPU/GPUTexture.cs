// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Extensions;
namespace CSDL.GPU {
    public class GPUTexture : NativeHandle<Opaque.SdlGPUTexture> {
        private readonly GPUDevice _gpuDevice;

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.CreateGPUTexture"/>
        public GPUTexture(GPUDevice gpuDevice, GPUTextureCreateInfo createInfo) {
            _gpuDevice = gpuDevice;
            Handle = SDL.CreateGPUTexture(gpuDevice.Handle, in createInfo).ThrowIfInvalid();
            Width = createInfo.Width;
            Height = createInfo.Height;
            gpuDevice.TrackChild(Invalidation);
        }

        internal GPUTexture(NativePtr<Opaque.SdlGPUTexture> texture, uint w, uint h, GPUDevice gpuDevice, bool isOwned = false) : base(texture, isOwned) {
            Width = w;
            Height = h;
            _gpuDevice = gpuDevice;
            gpuDevice.TrackChild(Invalidation);
        }

        public uint Width { get; private set; }
        public uint Height { get; private set; }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.SetGPUTextureName"/>
        public void SetGPUTextureName(string name) {
            SDL.SetGPUTextureName(_gpuDevice.Handle, Handle, name);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.ReleaseGPUTexture"/>
        protected override void DisposeResource() {
            SDL.ReleaseGPUTexture(_gpuDevice.Handle, Handle);
        }
    }
}
