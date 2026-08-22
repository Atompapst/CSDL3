// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Input;

namespace CSDL.EventHandlers.Interfaces {
    public partial interface IGamepadEvents {
        GamepadDeviceEvent? LastDeviceEvent { get; }
        int DeviceCount { get; }

        event Action<GamepadItem>? DeviceAdded;
        event Action<GamepadItem>? DeviceRemoved;
        event Action<GamepadItem>? DeviceRemapped;
    }
}

namespace CSDL.EventHandlers {
    internal sealed partial class Gamepad {
        private Counter _deviceCount;

        public GamepadDeviceEvent? LastDeviceEvent { get; private set; }
        public int DeviceCount => _deviceCount;

        public event Action<GamepadItem>? DeviceAdded;
        public event Action<GamepadItem>? DeviceRemoved;
        public event Action<GamepadItem>? DeviceRemapped;

        internal void Handle(GamepadDeviceEvent deviceEvent) {
            LastDeviceEvent = deviceEvent;
            IncrementCounter(ref _deviceCount);

            switch (deviceEvent.Type) {
                case EventType.GamepadAdded:
                {
                    Gamepads.OnGamepadAdded(deviceEvent.Which, deviceEvent.Timestamp);
                    EnsureAxisStateContainer(deviceEvent.Which);
                    EnsureButtonStateContainers(deviceEvent.Which);

                    GamepadItem? item = Gamepads.Get(deviceEvent.Which);
                    if (item != null) {
                        DeviceAdded?.Invoke(item);
                    }
                    break;
                }

                case EventType.GamepadRemoved:
                {
                    GamepadItem? item = Gamepads.Get(deviceEvent.Which);

                    RemoveAxisState(deviceEvent.Which);
                    RemoveButtonState(deviceEvent.Which);

                    Gamepads.OnGamepadRemoved(deviceEvent.Which);

                    if (item != null) {
                        DeviceRemoved?.Invoke(item);
                    }
                    break;
                }

                case EventType.GamepadRemapped:
                {
                    Gamepads.OnGamepadUpdated(deviceEvent.Which, deviceEvent.Timestamp);
                    GamepadItem? item = Gamepads.Get(deviceEvent.Which);
                    if (item != null) {
                        DeviceRemapped?.Invoke(item);
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
