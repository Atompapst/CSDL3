// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Extensions;
namespace CSDL.Video {

    public sealed class CameraDevice : NativeHandle<CSDL.Opaque.SdlCamera> {
        static CameraDevice() {
            Init.InitSubSystem(InitFlags.Camera);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Camera.OpenCamera"/>
        public CameraDevice(CameraID id, CameraSpec spec) {
            Handle = SDL.OpenCamera(id, spec).ThrowIfInvalid();
        }

        /// <summary>
        /// Opens the camera, letting SDL choose its native/default output format.
        /// </summary>
        /// <inheritdoc cref="CSDL.Internal.Docs.Camera.OpenCamera"/>
        public CameraDevice(CameraID id) {
            unsafe {
                Handle = SDL.OpenCameraNullable(id, null).ThrowIfInvalid();
            }
        }

        internal CameraDevice(NativePtr<CSDL.Opaque.SdlCamera> handle) : base(handle, false) {
            Handle = handle.ThrowIfInvalid();
        }
        public CameraID Id => GetCameraID();

        public CameraPermissionState PermissionState => GetPermissionState();

        /// <inheritdoc cref="CSDL.Internal.Docs.Camera.GetCameraProperties"/>
        public PropertiesID Properties => GetCameraProperties();

        /// <inheritdoc cref="CSDL.Internal.Docs.Camera.GetCameraFormat"/>
        public bool GetFormat(out CameraSpec spec) {
            spec = default;
            return SDL.GetCameraFormat(Handle, ref spec).LogIfFalse();
        }

        /// <summary>
        /// Acquires the next available camera frame, if one is ready.
        /// </summary>
        /// <remarks>
        /// Release the returned surface with <see cref="ReleaseFrame"/> once
        /// done with it - never dispose it directly, SDL owns it.
        /// </remarks>
        /// <inheritdoc cref="CSDL.Internal.Docs.Camera.AcquireCameraFrame"/>
        public Surface? AcquireFrame(out ulong timestampNs) {
            NativePtr<SurfaceData> ptr = SDL.AcquireCameraFrame(Handle, out timestampNs);
            return ptr.IsNull ? null : new Surface(ptr, false);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Camera.ReleaseCameraFrame"/>
        public void ReleaseFrame(Surface surface) {
            ArgumentNullException.ThrowIfNull(surface);
            SDL.ReleaseCameraFrame(Handle, surface.Handle);
            surface.Invalidate(); // zeroes the handle without freeing it - SDL already reclaimed the frame
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Camera.GetCameraPermissionState"/>
        private CameraPermissionState GetPermissionState() {
            return SDL.GetCameraPermissionState(Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Camera.GetCameraProperties"/>
        private PropertiesID GetCameraProperties() {
            return SDL.GetCameraProperties(Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Camera.GetCameraID"/>
        private CameraID GetCameraID() {
            return SDL.GetCameraID(Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Camera.CloseCamera"/>
        protected override void DisposeResource() {
            SDL.CloseCamera(Handle);
        }

        public override string ToString() {
            return $"CameraDevice(ID: {Id}, Permission: {PermissionState})";
        }
    }
}
