// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Extensions;
namespace CSDL.GPU {
    public class GPUBuffer : NativeHandle<Opaque.SdlGPUBuffer> {
        private readonly GPUDevice _gpuDevice;

        /// <inheritdoc cref="GPUBufferCreateInfo.Size"/>
        public uint SizeInBytes { get; }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.CreateGPUBuffer"/>
        public GPUBuffer(GPUDevice gpuDevice, GPUBufferCreateInfo createInfo) {
            _gpuDevice = gpuDevice;
            SizeInBytes = createInfo.Size;
            Handle = SDL.CreateGPUBuffer(gpuDevice.Handle, in createInfo).ThrowIfInvalid();
            gpuDevice.TrackChild(Invalidation);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.SetGPUBufferName"/>
        public void SetGPUBufferName(string name) {
            SDL.SetGPUBufferName(_gpuDevice.Handle, Handle, name);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.ReleaseGPUBuffer"/>
        protected override void DisposeResource() {
            SDL.ReleaseGPUBuffer(_gpuDevice.Handle, Handle);
        }
    }
}
