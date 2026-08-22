// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Extensions;
namespace CSDL.GPU {
    public class GPUShader : NativeHandle<Opaque.SdlGPUShader> {
        private readonly GPUDevice _gpuDevice;

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.CreateGPUShader"/>
        public GPUShader(GPUDevice gpuDevice, GPUShaderCreateInfo createInfo) {
            _gpuDevice = gpuDevice;
            Handle = SDL.CreateGPUShader(gpuDevice.Handle, in createInfo).ThrowIfInvalid();
            gpuDevice.TrackChild(Invalidation);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.ReleaseGPUShader"/>
        protected override void DisposeResource() {
            SDL.ReleaseGPUShader(_gpuDevice.Handle, Handle);
        }
    }
}
