// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;

namespace CSDL.EventHandlers.Interfaces {
    public partial interface IGamepadEvents {
        GamepadSensorEvent? LastSensorEvent { get; }
        int SensorCount { get; }

        event Action<GamepadSensorEvent>? SensorUpdated;
    }
}

namespace CSDL.EventHandlers {
    internal sealed partial class Gamepad {
        private Counter _sensorCount;

        public GamepadSensorEvent? LastSensorEvent { get; private set; }
        public int SensorCount => _sensorCount;

        public event Action<GamepadSensorEvent>? SensorUpdated;

        internal void Handle(GamepadSensorEvent sensorEvent) {
            Input.Gamepads.OnGamepadUpdated(sensorEvent.Which, sensorEvent.Timestamp);

            LastSensorEvent = sensorEvent;
            IncrementCounter(ref _sensorCount);

            SensorUpdated?.Invoke(sensorEvent);
        }

        partial void ResetSensorState() {
            if (!_sensorCount.HasEvents) return;
            _sensorCount.Reset();
        }
    }
}
