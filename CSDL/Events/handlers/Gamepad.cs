// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System.Collections.Generic;
using CSDL.Input;

namespace CSDL.EventHandlers.Interfaces {
    public partial interface IGamepadEvents {
        bool IsButtonDown(uint id, GamepadButton button);
        bool IsButtonUp(uint id, GamepadButton button);
        bool PressedThisCycle(uint id, GamepadButton button);
        bool ReleasedThisCycle(uint id, GamepadButton button);
        short GetAxis(uint id, GamepadAxis axis);
    }
}

namespace CSDL.EventHandlers {
    internal sealed partial class Gamepad : EventHandlerBase, Interfaces.IGamepadEvents {
        public bool IsButtonDown(uint id, GamepadButton button) {
            return _buttonStates.TryGetValue(id, out HashSet<GamepadButton>? buttons) && buttons.Contains(button);
        }

        public bool IsButtonUp(uint id, GamepadButton button) {
            return !IsButtonDown(id, button);
        }

        public bool PressedThisCycle(uint id, GamepadButton button) {
            return _pressedThisCycle.TryGetValue(id, out HashSet<GamepadButton>? buttons) && buttons.Contains(button);
        }

        public bool ReleasedThisCycle(uint id, GamepadButton button) {
            return _releasedThisCycle.TryGetValue(id, out HashSet<GamepadButton>? buttons) && buttons.Contains(button);
        }

        public short GetAxis(uint id, GamepadAxis axis) {
            if (_axisStates.TryGetValue(id, out Dictionary<GamepadAxis, short>? axes) &&
                axes.TryGetValue(axis, out short value)) {
                return value;
            }
            return 0;
        }

        protected override void ResetState() {
            ResetDeviceState();
            ResetAxisState();
            ResetButtonState();
            ResetTouchpadState();
            ResetSensorState();
            ResetCapSenseState();
        }

        partial void ResetDeviceState();
        partial void ResetAxisState();
        partial void ResetButtonState();
        partial void ResetTouchpadState();
        partial void ResetSensorState();
        partial void ResetCapSenseState();
    }
}
