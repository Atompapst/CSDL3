// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Extensions;
namespace CSDL.GPU {
    public class GPUSampler : NativeHandle<Opaque.SdlGPUSampler> {
        private readonly GPUDevice _gpuDevice;

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.CreateGPUSampler"/>
        public GPUSampler(GPUDevice gpuDevice, GPUSamplerCreateInfo createInfo) {
            _gpuDevice = gpuDevice;
            Handle = SDL.CreateGPUSampler(gpuDevice.Handle, in createInfo).ThrowIfInvalid();
            gpuDevice.TrackChild(Invalidation);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.ReleaseGPUSampler"/>
        protected override void DisposeResource() {
            SDL.ReleaseGPUSampler(_gpuDevice.Handle, Handle);
        }
    }
}
