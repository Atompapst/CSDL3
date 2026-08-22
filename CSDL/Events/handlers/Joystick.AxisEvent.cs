// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Collections.Generic;

namespace CSDL.EventHandlers.Interfaces {
    public partial interface IJoystickEvents {
        JoyAxisEvent? LastAxisEvent { get; }
        int AxisCount { get; }

        event Action<JoyAxisEvent>? AxisChanged;
    }
}

namespace CSDL.EventHandlers {
    internal sealed partial class Joystick {
        private readonly Dictionary<uint, Dictionary<byte, short>> _axisStates = new Dictionary<uint, Dictionary<byte, short>>();

        private Counter _axisCount;

        public JoyAxisEvent? LastAxisEvent { get; private set; }
        public int AxisCount => _axisCount;

        public event Action<JoyAxisEvent>? AxisChanged;

        internal void Handle(JoyAxisEvent axisEvent) {
            Input.Joysticks.OnJoystickUpdated(axisEvent.Which, axisEvent.Timestamp);
            EnsureAxisStateContainer(axisEvent.Which);

            _axisStates[axisEvent.Which][axisEvent.Axis] = axisEvent.Value;

            LastAxisEvent = axisEvent;
            IncrementCounter(ref _axisCount);

            AxisChanged?.Invoke(axisEvent);
        }

        private void EnsureAxisStateContainer(uint id) {
            if (!_axisStates.ContainsKey(id)) {
                _axisStates[id] = new Dictionary<byte, short>();
            }
        }

        private void RemoveAxisState(uint id) {
            _axisStates.Remove(id);
        }

        partial void ResetAxisState() {
            if (!_axisCount.HasEvents) return;
            _axisCount.Reset();
        }
    }
}
