// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;

namespace CSDL.EventHandlers.Interfaces {
    public partial interface IJoystickEvents {
        JoyBatteryEvent? LastBatteryEvent { get; }
        int BatteryCount { get; }

        event Action<JoyBatteryEvent>? BatteryUpdated;
    }
}

namespace CSDL.EventHandlers {
    internal sealed partial class Joystick {
        private Counter _batteryCount;

        public JoyBatteryEvent? LastBatteryEvent { get; private set; }
        public int BatteryCount => _batteryCount;

        public event Action<JoyBatteryEvent>? BatteryUpdated;

        internal void Handle(JoyBatteryEvent batteryEvent) {
            Input.Joysticks.OnJoystickUpdated(batteryEvent.Which, batteryEvent.Timestamp);

            LastBatteryEvent = batteryEvent;
            IncrementCounter(ref _batteryCount);

            BatteryUpdated?.Invoke(batteryEvent);
        }

        partial void ResetBatteryState() {
            if (!_batteryCount.HasEvents) return;
            _batteryCount.Reset();
        }
    }
}
