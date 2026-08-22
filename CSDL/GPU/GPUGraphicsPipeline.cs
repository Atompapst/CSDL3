// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Extensions;
namespace CSDL.GPU {

    public class GPUGraphicsPipeline : NativeHandle<Opaque.SdlGPUGraphicsPipeline> {
        private readonly GPUDevice _gpuDevice;

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.CreateGPUGraphicsPipeline"/>
        public GPUGraphicsPipeline(GPUDevice gpuDevice, in GPUGraphicsPipelineCreateInfo createInfo) {
            _gpuDevice = gpuDevice;
            Handle = SDL.CreateGPUGraphicsPipeline(gpuDevice.Handle, in createInfo).ThrowIfInvalid();
            gpuDevice.TrackChild(Invalidation);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.ReleaseGPUGraphicsPipeline"/>
        protected override void DisposeResource() {
            SDL.ReleaseGPUGraphicsPipeline(_gpuDevice.Handle, Handle);
        }
    }
}
