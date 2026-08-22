// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Extensions;
namespace CSDL.GPU {
    public class GPUComputePipeline : NativeHandle<Opaque.SdlGPUComputePipeline> {
        private readonly GPUDevice _gpuDevice;

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.CreateGPUComputePipeline"/>
        public GPUComputePipeline(GPUDevice gpuDevice, GPUComputePipelineCreateInfo createInfo) {
            _gpuDevice = gpuDevice;
            Handle = SDL.CreateGPUComputePipeline(gpuDevice.Handle, in createInfo).ThrowIfInvalid();
            gpuDevice.TrackChild(Invalidation);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.ReleaseGPUComputePipeline"/>
        protected override void DisposeResource() {
            SDL.ReleaseGPUComputePipeline(_gpuDevice.Handle, Handle);
        }
    }
}
