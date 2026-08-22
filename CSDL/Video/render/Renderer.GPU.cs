// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Extensions;
using CSDL.GPU;

namespace CSDL.Video {
    public partial class Renderer {
        /// <inheritdoc cref="CSDL.Internal.Docs.Render.GetGPURendererDevice"/>
        public GPUDevice? GPUDevice => GetGPUDevice();

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.CreateGPURenderer"/>
        /// <param name="device">the GPU device to render with, or <see langword="null"/> to let SDL create one.
        /// A device created by SDL is owned by the renderer and can be read back from <see cref="GPUDevice"/>.</param>
        /// <param name="window">the window to display in, or <see langword="null"/> for an offscreen renderer.
        /// In that case set a render target with <see cref="SetTarget"/> before drawing.</param>
        private void CreateGPU(GPUDevice? device = null, Window? window = null) {
            Handle = SDL.CreateGPURenderer(
                device?.Handle ?? NativePtr<CSDL.Opaque.SdlGPUDevice>.Zero,
                window?.Handle ?? NativePtr<CSDL.Opaque.SdlWindow>.Zero).ThrowIfInvalid();
            window?.RegisterChild(Invalidation);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.GetGPURendererDevice"/>
        private GPUDevice? GetGPUDevice() {
            NativePtr<CSDL.Opaque.SdlGPUDevice> device = SDL.GetGPURendererDevice(Handle);
            // The device belongs to the renderer - whoever passed it in owns it, and one SDL created
            // itself dies with the renderer - so the wrapper handed out here never owns the handle.
            return device.IsNull ? null : new GPUDevice(device, false);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.GetRenderMetalCommandEncoder"/>
        public IntPtr GetMetalCommandEncoder() {
            IntPtr ptr = SDL.GetRenderMetalCommandEncoder(Handle);
            return ptr;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.GetRenderMetalLayer"/>
        public IntPtr GetRenderMetalLayer() {
            IntPtr ptr = SDL.GetRenderMetalLayer(Handle);
            return ptr;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.AddVulkanRenderSemaphores"/>
        public void AddVulkanSemaphores(uint waitStageMask, long waitSemaphore, long signalSemaphore) {
            SDL.AddVulkanRenderSemaphores(Handle, waitStageMask, waitSemaphore, signalSemaphore).ThrowIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.CreateGPURenderState"/>
        public GPURenderState CreateGPUState(GPUShader fragmentShader, GPUTextureSamplerBinding[]? samplerBindings = null, GPUTexture[]? storageTextures = null, GPUBuffer[]? storageBuffers = null) {
            return new GPURenderState(this, fragmentShader, samplerBindings, storageTextures, storageBuffers);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.SetGPURenderState"/>
        public bool SetGPUState(GPURenderState? state) {
            return SDL.SetGPURenderState(Handle, state?.Handle ?? NativePtr<CSDL.Opaque.SdlGPURenderState>.Zero).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.GDKResumeRenderer"/>
        /// <remarks>
        /// Only exported by GDK (Xbox) builds of SDL; calling this against a non-GDK native
        /// library throws an <see cref="System.EntryPointNotFoundException"/>.
        /// </remarks>
        public void GdkResume() {
            SDL.GDKResumeRenderer(Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Render.GDKSuspendRenderer"/>
        /// <remarks>
        /// Only exported by GDK (Xbox) builds of SDL; calling this against a non-GDK native
        /// library throws an <see cref="System.EntryPointNotFoundException"/>.
        /// </remarks>
        public void GdkSuspend() {
            SDL.GDKSuspendRenderer(Handle);
        }

        private static string[] GetDrivers() {
            int count = SDL.GetNumRenderDrivers();
            string[] drivers = new string[count];
            for (int i = 0; i < count; i++) {
                drivers[i] = SDL.GetRenderDriver(i).ToUtf8StringOrLog() ?? string.Empty;
            }
            return drivers;
        }
    }
}
