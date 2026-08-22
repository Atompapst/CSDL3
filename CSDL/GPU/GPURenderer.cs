// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Extensions;
using System;
using System.Diagnostics.CodeAnalysis;
using CSDL.Video;
namespace CSDL.GPU {
    public sealed class GPURenderer : IDisposable {
        private readonly GPUDevice _gpuDevice;
        private readonly bool _ownsDevice;
        private Window? _window;
        private bool _windowClaimed;

        public GPUDevice GpuDevice => _gpuDevice;
        public Window? Window => _window;
        public bool HasWindow => _window != null;
        public bool IsWindowClaimed => _windowClaimed;

        public GPURenderer(GPUDevice gpuDevice, Window? window = null, bool claimWindow = true) {
            ArgumentNullException.ThrowIfNull(gpuDevice);
            _gpuDevice = gpuDevice;

            if (window != null) {
                _window = window;
                if (claimWindow) {
                    ClaimWindowInternal(window);
                }
            }
        }

        public GPURenderer(GPUShaderFormat shaderFormat, Window? window = null, bool debug = false, string? name = null, bool claimWindow = true)
            : this(new GPUDevice(shaderFormat, debug, name), window, claimWindow) {
            _ownsDevice = true;
        }

        public GPURenderer(GPUDeviceProperties properties, Window? window = null, bool claimWindow = true)
            : this(new GPUDevice(properties), window, claimWindow) {
            _ownsDevice = true;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.ClaimWindowForGPUDevice"/>
        public void ClaimWindow(Window window) {
            ArgumentNullException.ThrowIfNull(window);
            if (_windowClaimed && ReferenceEquals(_window, window)) {
                return;
            }
            if (_windowClaimed) {
                ReleaseWindow();
            }

            _window = window;
            ClaimWindowInternal(window);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.ReleaseWindowFromGPUDevice"/>
        public void ReleaseWindow() {
            if (!_windowClaimed || _window == null) return;
            _gpuDevice.ReleaseWindow(_window);
            _windowClaimed = false;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.CreateGPUBuffer"/>
        public GPUBuffer CreateBuffer(in GPUBufferCreateInfo createInfo) {
            return new GPUBuffer(_gpuDevice, createInfo);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.CreateGPUTexture"/>
        public GPUTexture CreateTexture(in GPUTextureCreateInfo createInfo) {
            return new GPUTexture(_gpuDevice, createInfo);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.CreateGPUShader"/>
        public GPUShader CreateShader(in GPUShaderCreateInfo createInfo) {
            return new GPUShader(_gpuDevice, createInfo);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.CreateGPUSampler"/>
        public GPUSampler CreateSampler(in GPUSamplerCreateInfo createInfo) {
            return new GPUSampler(_gpuDevice, createInfo);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.CreateGPUGraphicsPipeline"/>
        public GPUGraphicsPipeline CreateGraphicsPipeline(in GPUGraphicsPipelineCreateInfo createInfo) {
            return new GPUGraphicsPipeline(_gpuDevice, in createInfo);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.CreateGPUComputePipeline"/>
        public GPUComputePipeline CreateComputePipeline(in GPUComputePipelineCreateInfo createInfo) {
            return new GPUComputePipeline(_gpuDevice, createInfo);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.CreateGPUTransferBuffer"/>
        public GPUTransferBuffer CreateTransferBuffer(in GPUTransferBufferCreateInfo createInfo) {
            return new GPUTransferBuffer(_gpuDevice, createInfo);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.AcquireGPUCommandBuffer"/>
        public GPUCommandBuffer AcquireCommandBuffer() {
            return new GPUCommandBuffer(_gpuDevice);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.BeginGPURenderPass"/>
        public GPURenderPass BeginRenderPass(GPUCommandBuffer gpuCommandBuffer, GPUColorTargetInfo[]? colorTargetInfos = null, GPUDepthStencilTargetInfo? depthStencilTargetInfo = null) {
            EnsureCommandBuffer(gpuCommandBuffer);
            return new GPURenderPass(gpuCommandBuffer, colorTargetInfos, depthStencilTargetInfo);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.BeginGPUComputePass"/>
        public GPUComputePass BeginComputePass(GPUCommandBuffer gpuCommandBuffer, GPUStorageTextureReadWriteBinding[]? storageTextureBindings = null, GPUStorageBufferReadWriteBinding[]? storageBufferBindings = null) {
            EnsureCommandBuffer(gpuCommandBuffer);
            return new GPUComputePass(gpuCommandBuffer, storageTextureBindings, storageBufferBindings);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.BeginGPUCopyPass"/>
        public GPUCopyPass BeginCopyPass(GPUCommandBuffer gpuCommandBuffer) {
            EnsureCommandBuffer(gpuCommandBuffer);
            return new GPUCopyPass(gpuCommandBuffer);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.SubmitGPUCommandBufferAndAcquireFence"/>
        public GPUFence SubmitAndAcquireFence(GPUCommandBuffer gpuCommandBuffer) {
            EnsureCommandBuffer(gpuCommandBuffer);
            return gpuCommandBuffer.SubmitAndAcquireFence();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.WaitForGPUFences"/>
        public bool WaitForFences(bool waitAll, GPUFence[] fences) {
            if (fences == null || fences.Length == 0) return false;
            bool res = false;
            unsafe {
                fences.WithPointers((ptr, count) => {
                    res = SDL.WaitForGPUFences(_gpuDevice.Handle, waitAll, ptr, count).LogIfFalse();
                });
            }
            return res;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.QueryGPUFence"/>
        public bool QueryFence(GPUFence gpuFence) {
            ArgumentNullException.ThrowIfNull(gpuFence);
            return SDL.QueryGPUFence(_gpuDevice.Handle, gpuFence.Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.MapGPUTransferBuffer"/>
        public IntPtr MapTransferBuffer(GPUTransferBuffer gpuTransferBuffer, bool cycle = false) {
            ArgumentNullException.ThrowIfNull(gpuTransferBuffer);
            return gpuTransferBuffer.Map(cycle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.UnmapGPUTransferBuffer"/>
        public void UnmapTransferBuffer(GPUTransferBuffer gpuTransferBuffer) {
            ArgumentNullException.ThrowIfNull(gpuTransferBuffer);
            gpuTransferBuffer.Unmap();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.UploadToGPUTexture"/>
        public void UploadToTexture(GPUCopyPass gpuCopyPass, in GPUTextureTransferInfo source, in GPUTextureRegion destination, bool cycle = false) {
            ArgumentNullException.ThrowIfNull(gpuCopyPass);
            SDL.UploadToGPUTexture(gpuCopyPass.Handle, in source, in destination, cycle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.UploadToGPUBuffer"/>
        public void UploadToBuffer(GPUCopyPass gpuCopyPass, in GPUTransferBufferLocation source, in GPUBufferRegion destination, bool cycle = false) {
            ArgumentNullException.ThrowIfNull(gpuCopyPass);
            SDL.UploadToGPUBuffer(gpuCopyPass.Handle, in source, in destination, cycle);
        }

        /// <summary>
        /// Copies <paramref name="data"/> into <paramref name="gpuTransferBuffer"/> and uploads it to
        /// <paramref name="destination"/> in one call, replacing the usual
        /// map/<see cref="System.Runtime.InteropServices.Marshal.Copy(byte[],int,IntPtr,int)"/>/unmap/build-region
        /// dance with a single Span-based call.
        /// </summary>
        /// <typeparam name="T">An unmanaged element type.</typeparam>
        /// <param name="gpuCopyPass">The copy pass to record the upload into.</param>
        /// <param name="gpuTransferBuffer">A transfer buffer at least as large as <paramref name="data"/>; reused across calls if desired.</param>
        /// <param name="destination">The GPU buffer to upload into.</param>
        /// <param name="data">The source data.</param>
        /// <param name="destinationOffset">The byte offset into <paramref name="destination"/> to upload into.</param>
        /// <param name="cycleTransferBuffer">See the <c>cycle</c> parameter of <see cref="CSDL.Internal.Docs.GPU.MapGPUTransferBuffer"/>.</param>
        /// <param name="cycleDestination">See the <c>cycle</c> parameter of <see cref="CSDL.Internal.Docs.GPU.UploadToGPUBuffer"/>.</param>
        public unsafe void UploadToBuffer<T>(GPUCopyPass gpuCopyPass, GPUTransferBuffer gpuTransferBuffer, GPUBuffer destination, ReadOnlySpan<T> data, uint destinationOffset = 0, bool cycleTransferBuffer = false, bool cycleDestination = false) where T : unmanaged {
            ArgumentNullException.ThrowIfNull(gpuCopyPass);
            ArgumentNullException.ThrowIfNull(gpuTransferBuffer);
            ArgumentNullException.ThrowIfNull(destination);

            gpuTransferBuffer.SetData(data, cycle: cycleTransferBuffer);

            GPUTransferBufferLocation location = new GPUTransferBufferLocation { TransferBuffer = gpuTransferBuffer.NativePointer, Offset = 0 };
            GPUBufferRegion region = new GPUBufferRegion { Buffer = destination.NativePointer, Offset = destinationOffset, Size = (uint)(data.Length * sizeof(T)) };
            SDL.UploadToGPUBuffer(gpuCopyPass.Handle, in location, in region, cycleDestination);
        }

        /// <summary>
        /// Copies <paramref name="data"/> into <paramref name="gpuTransferBuffer"/> and uploads it to
        /// <paramref name="destination"/> in one call, tightly packed and covering the full extent of
        /// the destination texture. For partial/strided uploads, use the
        /// <c>UploadToTexture(CopyPass, in GPUTextureTransferInfo, in GPUTextureRegion, bool)</c> overload directly.
        /// </summary>
        /// <typeparam name="T">An unmanaged element type.</typeparam>
        /// <param name="gpuCopyPass">The copy pass to record the upload into.</param>
        /// <param name="gpuTransferBuffer">A transfer buffer at least as large as <paramref name="data"/>; reused across calls if desired.</param>
        /// <param name="destination">The GPU texture to upload into.</param>
        /// <param name="data">The source pixel/texel data, tightly packed.</param>
        /// <param name="mipLevel">The mip level to upload into.</param>
        /// <param name="layer">The layer/depth-slice to upload into.</param>
        /// <param name="cycleTransferBuffer">See the <c>cycle</c> parameter of <see cref="CSDL.Internal.Docs.GPU.MapGPUTransferBuffer"/>.</param>
        /// <param name="cycleDestination">See the <c>cycle</c> parameter of <see cref="CSDL.Internal.Docs.GPU.UploadToGPUTexture"/>.</param>
        public void UploadToTexture<T>(GPUCopyPass gpuCopyPass, GPUTransferBuffer gpuTransferBuffer, GPUTexture destination, ReadOnlySpan<T> data, uint mipLevel = 0, uint layer = 0, bool cycleTransferBuffer = false, bool cycleDestination = false) where T : unmanaged {
            ArgumentNullException.ThrowIfNull(gpuCopyPass);
            ArgumentNullException.ThrowIfNull(gpuTransferBuffer);
            ArgumentNullException.ThrowIfNull(destination);

            gpuTransferBuffer.SetData(data, cycle: cycleTransferBuffer);

            GPUTextureTransferInfo source = new GPUTextureTransferInfo {
                TransferBuffer = gpuTransferBuffer.NativePointer,
                Offset = 0,
                PixelsPerRow = 0,
                RowsPerLayer = 0,
            };
            GPUTextureRegion destRegion = new GPUTextureRegion {
                Texture = destination.NativePointer,
                MipLevel = mipLevel,
                Layer = layer,
                X = 0, Y = 0, Z = 0,
                W = destination.Width, H = destination.Height, D = 1,
            };
            SDL.UploadToGPUTexture(gpuCopyPass.Handle, in source, in destRegion, cycleDestination);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.CopyGPUTextureToTexture"/>
        public void CopyTextureToTexture(GPUCopyPass gpuCopyPass, in GPUTextureLocation source, in GPUTextureLocation destination, uint width, uint height, uint depth, bool cycle = false) {
            ArgumentNullException.ThrowIfNull(gpuCopyPass);
            SDL.CopyGPUTextureToTexture(gpuCopyPass.Handle, in source, in destination, width, height, depth, cycle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.CopyGPUBufferToBuffer"/>
        public void CopyBufferToBuffer(GPUCopyPass gpuCopyPass, in GPUBufferLocation source, in GPUBufferLocation destination, uint size, bool cycle = false) {
            ArgumentNullException.ThrowIfNull(gpuCopyPass);
            SDL.CopyGPUBufferToBuffer(gpuCopyPass.Handle, in source, in destination, size, cycle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.DownloadFromGPUTexture"/>
        public void DownloadFromTexture(GPUCopyPass gpuCopyPass, in GPUTextureRegion source, in GPUTextureTransferInfo destination) {
            ArgumentNullException.ThrowIfNull(gpuCopyPass);
            SDL.DownloadFromGPUTexture(gpuCopyPass.Handle, in source, in destination);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.PushGPUVertexUniformData"/>
        public void PushVertexUniformData(GPUCommandBuffer gpuCommandBuffer, uint slotIndex, byte[] data) {
            EnsureCommandBuffer(gpuCommandBuffer);
            if (data == null || data.Length == 0) return;
            unsafe {
                fixed (byte* b = data) {
                    SDL.PushGPUVertexUniformData(gpuCommandBuffer.Handle, slotIndex, (nint)b, (uint)data.Length);
                }
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.PushGPUFragmentUniformData"/>
        public void PushFragmentUniformData(GPUCommandBuffer gpuCommandBuffer, uint slotIndex, byte[] data) {
            EnsureCommandBuffer(gpuCommandBuffer);
            if (data == null || data.Length == 0) return;
            unsafe {
                fixed (byte* b = data) {
                    SDL.PushGPUFragmentUniformData(gpuCommandBuffer.Handle, slotIndex, (nint)b, (uint)data.Length);
                }
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.PushGPUComputeUniformData"/>
        public void PushComputeUniformData(GPUCommandBuffer gpuCommandBuffer, uint slotIndex, byte[] data) {
            EnsureCommandBuffer(gpuCommandBuffer);
            if (data == null || data.Length == 0) return;
            unsafe {
                fixed (byte* b = data) {
                    SDL.PushGPUComputeUniformData(gpuCommandBuffer.Handle, slotIndex, (nint)b, (uint)data.Length);
                }
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.WindowSupportsGPUSwapchainComposition"/>
        public bool WindowSupportsSwapchainComposition(GPUSwapchainComposition swapchainComposition) {
            EnsureWindow();
            return SDL.WindowSupportsGPUSwapchainComposition(_gpuDevice.Handle, _window.Handle, swapchainComposition);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.WindowSupportsGPUPresentMode"/>
        public bool WindowSupportsPresentMode(GPUPresentMode presentMode) {
            EnsureWindow();
            return SDL.WindowSupportsGPUPresentMode(_gpuDevice.Handle, _window.Handle, presentMode);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.SetGPUSwapchainParameters"/>
        public bool SetSwapchainParameters(GPUSwapchainComposition swapchainComposition, GPUPresentMode presentMode) {
            EnsureWindow();
            return SDL.SetGPUSwapchainParameters(_gpuDevice.Handle, _window.Handle, swapchainComposition, presentMode).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.GetGPUSwapchainTextureFormat"/>
        public GPUTextureFormat SwapchainTextureFormat {
            get {
                EnsureWindow();
                return SDL.GetGPUSwapchainTextureFormat(_gpuDevice.Handle, _window.Handle);
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.WaitForGPUSwapchain"/>
        public bool WaitForSwapchain() {
            EnsureWindow();
            return SDL.WaitForGPUSwapchain(_gpuDevice.Handle, _window.Handle).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.AcquireGPUSwapchainTexture"/>
        public bool AcquireSwapchainTexture(GPUCommandBuffer gpuCommandBuffer, out GPUTexture? swapchainGpuTexture) {
            EnsureCommandBuffer(gpuCommandBuffer);
            gpuCommandBuffer.EnsureOutsidePass();
            EnsureWindow();
            bool res = SDL.AcquireGPUSwapchainTexture(gpuCommandBuffer.Handle, _window.Handle, out NativePtr<Opaque.SdlGPUTexture> texture, out uint width, out uint height).LogIfFalse();
            if (!res || texture.IsNull) {
                swapchainGpuTexture = null;
            } else {
                // isOwned: false -- the swapchain texture is managed internally by SDL_GPU and must never
                // be released by application code (SDL_ReleaseGPUTexture on it is invalid).
                swapchainGpuTexture = new GPUTexture(texture, width, height, _gpuDevice, false);
                gpuCommandBuffer.MarkSwapchainTextureAcquired();
            }
            return res;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.WaitAndAcquireGPUSwapchainTexture"/>
        public bool WaitAndAcquireSwapchainTexture(GPUCommandBuffer gpuCommandBuffer, out GPUTexture? swapchainGpuTexture) {
            EnsureCommandBuffer(gpuCommandBuffer);
            gpuCommandBuffer.EnsureOutsidePass();
            EnsureWindow();
            bool res = SDL.WaitAndAcquireGPUSwapchainTexture(gpuCommandBuffer.Handle, _window.Handle, out NativePtr<Opaque.SdlGPUTexture> texture, out uint width, out uint height).LogIfFalse();
            if (!res || texture.IsNull) {
                swapchainGpuTexture = null;
            } else {
                // isOwned: false -- the swapchain texture is managed internally by SDL_GPU and must never
                // be released by application code (SDL_ReleaseGPUTexture on it is invalid).
                swapchainGpuTexture = new GPUTexture(texture, width, height, _gpuDevice, false);
                gpuCommandBuffer.MarkSwapchainTextureAcquired();
            }
            return res;
        }

        /// <summary>
        /// If the window was claimed by this instance, it will be released, and if the instance owns the underlying device, the device will also be disposed.
        /// </summary>
        public void Dispose() {
            if (_windowClaimed) {
                ReleaseWindow();
            }
            if (_ownsDevice) {
                _gpuDevice.Dispose();
            }
        }

        private void ClaimWindowInternal(Window window) {
            _gpuDevice.ClaimWindow(window);
            _windowClaimed = true;
        }

        [MemberNotNull(nameof(_window))]
        private void EnsureWindow() {
            if (_window == null) {
                throw new InvalidOperationException("GpuRenderer requires a Window for swapchain operations.");
            }
        }

        private void EnsureCommandBuffer(GPUCommandBuffer gpuCommandBuffer) {
            ArgumentNullException.ThrowIfNull(gpuCommandBuffer);
            if (!ReferenceEquals(gpuCommandBuffer.Device, _gpuDevice)) {
                throw new ArgumentException("The command buffer was acquired from a different GPU device.", nameof(gpuCommandBuffer));
            }
        }
    }
}
