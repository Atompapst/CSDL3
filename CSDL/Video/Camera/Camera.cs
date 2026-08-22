// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System.Collections.Generic;
using CSDL.Extensions;
namespace CSDL.Video {
    public static class Camera {
        private static readonly Dictionary<CameraID, CameraItem> _cameras = new Dictionary<CameraID, CameraItem>();

        static Camera() {
            Init.InitSubSystem(InitFlags.Camera);
            Refresh();
        }
        public static IReadOnlyCollection<CameraItem> All => _cameras.Values;

        /// <inheritdoc cref="CSDL.Internal.Docs.Camera.GetNumCameraDrivers"/>
        public static int DriverCount => SDL.GetNumCameraDrivers();

        /// <inheritdoc cref="CSDL.Internal.Docs.Camera.GetCurrentCameraDriver"/>
        public static string CurrentDriver => SDL.GetCurrentCameraDriver().ToUtf8String() ?? string.Empty;

        /// <inheritdoc cref="CSDL.Internal.Docs.Camera.GetCameras"/>
        private static void Refresh() {
            NativePtr<CameraID> ids = SDL.GetCameras(out int count).LogIfInvalid();
            if (ids.IsNull) return;

            for (int i = 0; i < count; i++) {
                CameraID id = ids[i];
                if (!_cameras.ContainsKey(id)) {
                    _cameras[id] = new CameraItem(id);
                }
            }
        }

        internal static void OnCameraAdded(CameraID id, ulong timestamp) {
            if (!_cameras.ContainsKey(id)) {
                _cameras[id] = new CameraItem(id);
            }
        }

        internal static void OnCameraRemoved(CameraID id) {
            _cameras.Remove(id);
        }

        internal static void OnCameraUpdated(CameraID id, ulong timestamp) {
            if (_cameras.TryGetValue(id, out CameraItem? item)) {
                item.LastTimestampNs = timestamp;
            }
        }

        public static bool IsPresent(CameraID id) {
            return _cameras.ContainsKey(id);
        }
        public static CameraItem? Get(CameraID id) {
            return _cameras.GetValueOrDefault(id);
        }
        /// <inheritdoc cref="CSDL.Internal.Docs.Camera.GetCameraDriver"/>
        public static string GetDriver(int index) {
            return SDL.GetCameraDriver(index).ToUtf8String() ?? string.Empty;
        }
    }
}
