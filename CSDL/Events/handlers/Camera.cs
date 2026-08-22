// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;

namespace CSDL.EventHandlers.Interfaces {
    public interface ICameraEvents {
        event Action<Video.CameraItem>? DeviceAdded;
        event Action<Video.CameraItem>? DeviceRemoved;
        event Action<Video.CameraItem>? DeviceApproved;
        event Action<Video.CameraItem>? DeviceDenied;
    }
}

namespace CSDL.EventHandlers {
    internal sealed class Camera : Interfaces.ICameraEvents {
        public event Action<Video.CameraItem>? DeviceAdded;
        public event Action<Video.CameraItem>? DeviceRemoved;
        public event Action<Video.CameraItem>? DeviceApproved;
        public event Action<Video.CameraItem>? DeviceDenied;

        internal void Handle(CameraDeviceEvent deviceEvent) {
            switch (deviceEvent.Type) {
                case EventType.CameraDeviceAdded:
                {
                    Video.Camera.OnCameraAdded(deviceEvent.Which, deviceEvent.Timestamp);
                    Video.CameraItem? item = Video.Camera.Get(deviceEvent.Which);
                    if (item != null) DeviceAdded?.Invoke(item);
                    break;
                }
                case EventType.CameraDeviceRemoved:
                {
                    Video.CameraItem? item = Video.Camera.Get(deviceEvent.Which);
                    Video.Camera.OnCameraRemoved(deviceEvent.Which);
                    if (item != null) DeviceRemoved?.Invoke(item);
                    break;
                }
                case EventType.CameraDeviceApproved:
                {
                    Video.Camera.OnCameraUpdated(deviceEvent.Which, deviceEvent.Timestamp);
                    Video.CameraItem? item = Video.Camera.Get(deviceEvent.Which);
                    if (item != null) DeviceApproved?.Invoke(item);
                    break;
                }
                case EventType.CameraDeviceDenied:
                {
                    Video.Camera.OnCameraUpdated(deviceEvent.Which, deviceEvent.Timestamp);
                    Video.CameraItem? item = Video.Camera.Get(deviceEvent.Which);
                    if (item != null) DeviceDenied?.Invoke(item);
                    break;
                }
            }
        }
    }
}
