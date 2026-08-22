// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Extensions;
namespace CSDL.Video {

    public sealed class CameraItem {

        internal CameraItem(CameraID id) {
            Id = id;
        }
        public CameraID Id { get; }
        public ulong LastTimestampNs { get; internal set; }
        public string Name => GetName();
        public CameraPosition Position => GetPosition();

        /// <inheritdoc cref="CSDL.Internal.Docs.Camera.GetCameraSupportedFormats"/>
        public CameraSpec[] SupportedFormats => GetSupportedFormats();

        /// <summary>
        /// Opens the camera, letting SDL choose its native/default output format.
        /// </summary>
        public CameraDevice Open() {
            return new CameraDevice(Id);
        }

        public CameraDevice Open(CameraSpec format) {
            return new CameraDevice(Id, format);
        }


        /// <inheritdoc cref="CSDL.Internal.Docs.Camera.GetCameraName"/>
        private string GetName() {
            return SDL.GetCameraName(Id).ToUtf8StringOrLog() ?? "Unknown Camera";
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Camera.GetCameraPosition"/>
        private CameraPosition GetPosition() {
            return SDL.GetCameraPosition(Id);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Camera.GetCameraSupportedFormats"/>
        private CameraSpec[] GetSupportedFormats() {
            IntPtr ptr = SDL.GetCameraSupportedFormats(Id, out int count);
            if (ptr == IntPtr.Zero) {
                Error.LogError(nameof(GetSupportedFormats));
                return Array.Empty<CameraSpec>();
            }

            NativePtr<NativePtr<CameraSpec>> formats = ptr;
            CameraSpec[] result = new CameraSpec[count];
            for (int i = 0; i < count; i++) {
                result[i] = formats[i].Read();
            }
            Memory.Free(ptr);
            return result;
        }

        public override string ToString() {
            return $"{Name} [{Position}] (ID: {Id})";
        }
    }
}
