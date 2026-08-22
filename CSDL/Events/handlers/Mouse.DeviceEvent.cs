// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;

namespace CSDL.EventHandlers.Interfaces {
    public partial interface IMouseEvents {
        int DeviceCount { get; }
        MouseDeviceEvent LastDeviceEvent { get; }

        event Action<MouseDeviceEvent>? DeviceAdded;
        event Action<MouseDeviceEvent>? DeviceRemoved;
    }
}

namespace CSDL.EventHandlers {
    internal sealed partial class Mouse {
        private Counter _deviceCount;

        public int DeviceCount => _deviceCount;
        public MouseDeviceEvent LastDeviceEvent { get; private set; }

        public event Action<MouseDeviceEvent>? DeviceAdded;
        public event Action<MouseDeviceEvent>? DeviceRemoved;

        internal void Handle(MouseDeviceEvent deviceEvent) {
            switch (deviceEvent.Type) {
                case EventType.MouseAdded:
                    Input.Mouse.OnMouseAdded(deviceEvent.Which, deviceEvent.Timestamp);
                    DeviceAdded?.Invoke(deviceEvent);
                    break;

                case EventType.MouseRemoved:
                    Input.Mouse.OnMouseRemoved(deviceEvent.Which);
                    DeviceRemoved?.Invoke(deviceEvent);
                    break;
            }

            LastDeviceEvent = deviceEvent;
            IncrementCounter(ref _deviceCount);
        }

        partial void ResetDeviceState() {
            if (!_deviceCount.HasEvents) return;
            _deviceCount.Reset();
        }
    }
}
