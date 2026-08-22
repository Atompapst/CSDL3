// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

namespace CSDL.GPU {

    public class GPUFence : NativeHandle<Opaque.SdlGPUFence> {
        private readonly GPUDevice _gpuDevice;

        internal GPUFence(GPUDevice gpuDevice, NativePtr<Opaque.SdlGPUFence> handle) {
            _gpuDevice = gpuDevice;
            Handle = handle;
            gpuDevice.TrackChild(Invalidation);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.ReleaseGPUFence"/>
        protected override void DisposeResource() {
            SDL.ReleaseGPUFence(_gpuDevice.Handle, Handle);
        }
    }
}
