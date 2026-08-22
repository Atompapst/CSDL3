// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Extensions;
using CSDL.Video;
using System;
using System.Collections.Generic;
using System.Linq;
namespace CSDL.GPU {

    public class GPUDevice : NativeHandle<Opaque.SdlGPUDevice> {
        private readonly object _childrenLock = new();
        private readonly List<WeakReference<Internal.InvalidationRegistration>> _children = [];
        /// <summary>
        /// Get the GPU drivers compiled into SDL.
        /// </summary>
        /// <seealso cref="CSDL.Internal.Docs.GPU.GetNumGPUDrivers">GetNumGPUDrivers</seealso>
        /// <seealso cref="CSDL.Internal.Docs.GPU.GetGPUDriver">GetGPUDriver</seealso>
        public static string[] Drivers => GetDrivers();
        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.GetGPUShaderFormats"/>
        public GPUShaderFormat ActiveDrivers => SDL.GetGPUShaderFormats(Handle);

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.GetGPUDeviceDriver"/>
        public string? Driver => SDL.GetGPUDeviceDriver(Handle).ToUtf8StringOrLog();

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.GetGPUDeviceProperties"/>
        /// <remarks>
        /// The returned group is owned by SDL and describes this device; it is read-only and must
        /// not be disposed.
        /// </remarks>
        public GPUDeviceInfo Info => new GPUDeviceInfo(SDL.GetGPUDeviceProperties(Handle));
        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.CreateGPUDevice"/>
        public GPUDevice(GPUShaderFormat shaderFormat, bool debug = false, string? name = null) {
            Handle = SDL.CreateGPUDevice(shaderFormat, debug, name).ThrowIfInvalid();
        }
        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.CreateGPUDeviceWithProperties"/>
        public GPUDevice(GPUDeviceProperties properties) {
            Handle = SDL.CreateGPUDeviceWithProperties(properties.Handle).ThrowIfInvalid();
        }

        public GPUDevice(NativePtr<Opaque.SdlGPUDevice> device, bool ownsHandle = true) : base(device, ownsHandle) {
            Handle = device;
        }

        internal void TrackChild(Internal.InvalidationRegistration child) {
            lock (_childrenLock) {
                _children.RemoveAll(static reference => !reference.TryGetTarget(out _));
                _children.Add(new WeakReference<Internal.InvalidationRegistration>(child));
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.ClaimWindowForGPUDevice"/>
        public void ClaimWindow(Window window) {
            if (!SDL.ClaimWindowForGPUDevice(Handle, window.Handle)) {
                Error.ThrowIfError(nameof(ClaimWindow));
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.ReleaseWindowFromGPUDevice"/>
        public void ReleaseWindow(Window window) {
            SDL.ReleaseWindowFromGPUDevice(Handle, window.Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.GPUSupportsProperties"/>
        public static bool SupportsProperties(GPUDeviceProperties properties) {
            return SDL.GPUSupportsProperties(properties.Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.GPUSupportsShaderFormats"/>
        public static bool SupportsShaderFormats(GPUShaderFormat formatFlags, string? name = null) {
            return SDL.GPUSupportsShaderFormats(formatFlags, name);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.GDKResumeGPU"/>
        /// <remarks>
        /// Only exported by GDK (Xbox) builds of SDL; calling this against a non-GDK native
        /// library throws an <see cref="System.EntryPointNotFoundException"/>.
        /// </remarks>
        public void GdkResume() {
            SDL.GDKResumeGPU(Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.GDKSuspendGPU"/>
        /// <remarks>
        /// Only exported by GDK (Xbox) builds of SDL; calling this against a non-GDK native
        /// library throws an <see cref="System.EntryPointNotFoundException"/>.
        /// </remarks>
        public void GdkSuspend() {
            SDL.GDKSuspendGPU(Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.GPUTextureSupportsFormat"/>
        public bool TextureSupportsFormat(GPUTextureFormat format, GPUTextureType type, GPUTextureUsageFlags usage) {
            return SDL.GPUTextureSupportsFormat(Handle, format, type, usage);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.GPUTextureSupportsSampleCount"/>
        public bool TextureSupportsSampleCount(GPUTextureFormat format, GPUSampleCount sampleCount) {
            return SDL.GPUTextureSupportsSampleCount(Handle, format, sampleCount);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.SetGPUAllowedFramesInFlight"/>
        public bool SetAllowedFramesInFlight(uint allowedFramesInFlight) {
            return SDL.SetGPUAllowedFramesInFlight(Handle, allowedFramesInFlight).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.WaitForGPUIdle"/>
        public bool WaitForIdle() {
            return SDL.WaitForGPUIdle(Handle).LogIfFalse();
        }

        /// <seealso cref="CSDL.Internal.Docs.GPU.GetNumGPUDrivers">GetNumGPUDrivers</seealso>
        /// <seealso cref="CSDL.Internal.Docs.GPU.GetGPUDriver">GetGPUDriver</seealso>
        private static string[] GetDrivers() {
            int count = SDL.GetNumGPUDrivers();
            string[] formats = new string[count];
            for (int i = 0; i < count; i++) {
                formats[i] = SDL.GetGPUDriver(i).ToUtf8StringOrLog() ?? string.Empty;
            }
            return formats;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.GPU.DestroyGPUDevice"/>
        protected override void DisposeResource() {
            Internal.InvalidationRegistration[] children;
            lock (_childrenLock) {
                children = _children
                    .ConvertAll(static reference => reference.TryGetTarget(out Internal.InvalidationRegistration? child) ? child : null)
                    .Where(static child => child != null)
                    .Cast<Internal.InvalidationRegistration>()
                    .ToArray();
                _children.Clear();
            }

            foreach (Internal.InvalidationRegistration child in children) {
                child.Invalidate();
            }
            SDL.DestroyGPUDevice(Handle);
        }
    }
}
