// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Collections.Generic;
using CSDL.Input;

namespace CSDL.EventHandlers.Interfaces {
    public partial interface IGamepadEvents {
        GamepadAxisEvent? LastAxisEvent { get; }
        int AxisCount { get; }

        event Action<GamepadAxisEvent>? AxisChanged;
    }
}

namespace CSDL.EventHandlers {
    internal sealed partial class Gamepad {
        private readonly Dictionary<uint, Dictionary<GamepadAxis, short>> _axisStates = new Dictionary<uint, Dictionary<GamepadAxis, short>>();

        private Counter _axisCount;

        public GamepadAxisEvent? LastAxisEvent { get; private set; }
        public int AxisCount => _axisCount;

        public event Action<GamepadAxisEvent>? AxisChanged;

        internal void Handle(GamepadAxisEvent axisEvent) {
            Gamepads.OnGamepadUpdated(axisEvent.Which, axisEvent.Timestamp);
            EnsureAxisStateContainer(axisEvent.Which);

            _axisStates[axisEvent.Which][(GamepadAxis)axisEvent.Axis] = axisEvent.Value;

            LastAxisEvent = axisEvent;
            IncrementCounter(ref _axisCount);

            AxisChanged?.Invoke(axisEvent);
        }

        private void EnsureAxisStateContainer(uint id) {
            if (!_axisStates.ContainsKey(id)) {
                _axisStates[id] = new Dictionary<GamepadAxis, short>();
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
