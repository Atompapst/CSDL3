// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Input;

namespace CSDL.EventHandlers.Interfaces {
    public partial interface IJoystickEvents {
        JoyDeviceEvent? LastDeviceEvent { get; }
        int DeviceCount { get; }

        event Action<JoystickItem>? DeviceAdded;
        event Action<JoystickItem>? DeviceRemoved;
    }
}

namespace CSDL.EventHandlers {
    internal sealed partial class Joystick {
        private Counter _deviceCount;

        public JoyDeviceEvent? LastDeviceEvent { get; private set; }
        public int DeviceCount => _deviceCount;

        public event Action<JoystickItem>? DeviceAdded;
        public event Action<JoystickItem>? DeviceRemoved;

        internal void Handle(JoyDeviceEvent deviceEvent) {
            IncrementCounter(ref _deviceCount);
            LastDeviceEvent = deviceEvent;

            switch (deviceEvent.Type) {
                case EventType.JoystickAdded:
                {
                    Joysticks.OnJoystickAdded(deviceEvent.Which, deviceEvent.Timestamp);
                    EnsureAxisStateContainer(deviceEvent.Which);
                    EnsureButtonStateContainers(deviceEvent.Which);
                    EnsureHatStateContainer(deviceEvent.Which);
                    JoystickItem? added = Joysticks.Get(deviceEvent.Which);
                    if (added != null) {
                        DeviceAdded?.Invoke(added);
                    }
                    break;
                }
                case EventType.JoystickRemoved:
                {
                    JoystickItem? item = Joysticks.Get(deviceEvent.Which);

                    RemoveAxisState(deviceEvent.Which);
                    RemoveButtonState(deviceEvent.Which);
                    RemoveHatState(deviceEvent.Which);

                    Joysticks.OnJoystickRemoved(deviceEvent.Which);
                    if (item != null) {
                        DeviceRemoved?.Invoke(item);
                    }
                    break;
                }
            }
        }

        partial void ResetDeviceState() {
            if (!_deviceCount.HasEvents) return;
            _deviceCount.Reset();
        }
    }
}
